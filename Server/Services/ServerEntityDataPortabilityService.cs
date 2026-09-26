using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using GIBS.Module.Entity.Models;
using GIBS.Module.Entity.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Shared;

namespace GIBS.Module.Entity.Services
{
    public class ServerEntityDataPortabilityService : IEntityDataPortabilityService
    {
        private readonly IDbContextFactory<EntityContext> _factory;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly IFileRepository _fileRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly Alias _alias;
        private Dictionary<int, Oqtane.Models.File> _importedFilesBySourceId = new();
        private HashSet<int> _zipFileSourceIds = new();
        private HashSet<int> _missingZipFileEntryIds = new();

        public ServerEntityDataPortabilityService(
            IDbContextFactory<EntityContext> factory,
            IUserPermissions userPermissions,
            ITenantManager tenantManager,
            ILogManager logger,
            IHttpContextAccessor accessor,
            IFileRepository fileRepository,
            IFolderRepository folderRepository)
        {
            _factory = factory;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _fileRepository = fileRepository;
            _folderRepository = folderRepository;
            _alias = tenantManager.GetAlias();
        }

        public async Task<EntityDataPackage> ExportDataAsync(int siteId, int moduleId)
        {
            if (!_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityData export attempt {SiteId} {ModuleId}", siteId, moduleId);
                return null;
            }

            using var db = _factory.CreateDbContext();
            var package = new EntityDataPackage();

            var entityTypes = await db.EntityTypes
                .AsNoTracking()
                .Where(t => t.SiteId == siteId && t.ModuleId == moduleId)
                .ToDictionaryAsync(t => t.EntityTypeId, t => t);

            var records = await db.Entity
                .AsNoTracking()
                .Where(e => e.SiteId == siteId && e.ModuleId == moduleId)
                .OrderBy(e => e.SortOrder)
                .ThenBy(e => e.Name)
                .ToListAsync();

            var recordById = records.ToDictionary(r => r.EntityId, r => r);
            var recordKeysById = records.ToDictionary(r => r.EntityId, r => r.Key ?? string.Empty);

            var recordIds = records.Select(r => r.EntityId).ToList();
            var values = await db.EntityValues
                .AsNoTracking()
                .Where(v => recordIds.Contains(v.EntityId))
                .OrderBy(v => v.EntityId)
                .ThenBy(v => v.FieldId)
                .ThenBy(v => v.ValueIndex)
                .ToListAsync();

            var fieldIds = values.Select(v => v.FieldId).Distinct().ToList();
            var fieldsById = await db.EntityFields
                .AsNoTracking()
                .Where(f => fieldIds.Contains(f.FieldId))
                .ToDictionaryAsync(f => f.FieldId, f => f);

            var valuesByEntityId = values.GroupBy(v => v.EntityId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var record in records)
            {
                if (!entityTypes.TryGetValue(record.EntityTypeId, out var entityType))
                {
                    continue;
                }

                var recordItem = new EntityDataRecordItem
                {
                    EntityTypeKey = entityType.Key,
                    EntityKey = record.Key,
                    ParentEntityKey = record.ParentEntityId.HasValue && recordKeysById.TryGetValue(record.ParentEntityId.Value, out var parentKey)
                        ? parentKey
                        : null,
                    Name = record.Name,
                    Description = record.Description,
                    Status = record.Status,
                    IsEnabled = record.IsEnabled,
                    IsFeatured = record.IsFeatured,
                    IsPublished = record.IsPublished,
                    PublishStartDate = record.PublishStartDate,
                    PublishEndDate = record.PublishEndDate,
                    SortOrder = record.SortOrder,
                    Settings = record.Settings
                };

                if (valuesByEntityId.TryGetValue(record.EntityId, out var recordValues))
                {
                    foreach (var value in recordValues)
                    {
                        if (!fieldsById.TryGetValue(value.FieldId, out var field))
                        {
                            continue;
                        }

                        recordItem.Values.Add(new EntityDataValueItem
                        {
                            FieldKey = field.Key,
                            ValueIndex = value.ValueIndex,
                            TextValue = value.TextValue,
                            IntegerValue = value.IntegerValue,
                            LongValue = value.LongValue,
                            DecimalValue = value.DecimalValue,
                            BooleanValue = value.BooleanValue,
                            DateValue = value.DateValue,
                            DateTimeValue = value.DateTimeValue,
                            GuidValue = value.GuidValue,
                            ReferencedEntityKey = value.ReferencedEntityId.HasValue && recordKeysById.TryGetValue(value.ReferencedEntityId.Value, out var referencedKey)
                                ? referencedKey
                                : null
                        });
                    }
                }

                package.Records.Add(recordItem);
            }

            return package;
        }

        public async Task<EntityDataImportResult> ImportDataAsync(int siteId, int moduleId, EntityDataPackage package, bool overwriteExisting = true)
        {
            var result = new EntityDataImportResult();
            if (!_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityData import attempt {SiteId} {ModuleId}", siteId, moduleId);
                result.Warnings.Add("Unauthorized import attempt.");
                return result;
            }

            if (package?.Records == null || package.Records.Count == 0)
            {
                result.Warnings.Add("Import package is empty.");
                return result;
            }

            using var db = _factory.CreateDbContext();
            var now = DateTime.UtcNow;
            var user = _accessor.HttpContext?.User?.Identity?.Name ?? "system";

            var entityTypes = await db.EntityTypes
                .Where(t => t.SiteId == siteId && t.ModuleId == moduleId)
                .ToDictionaryAsync(t => t.Key, t => t, StringComparer.OrdinalIgnoreCase);

            var existingRecords = await db.Entity
                .Where(e => e.SiteId == siteId && e.ModuleId == moduleId)
                .ToListAsync();

            string BuildRecordScopeKey(string entityTypeKey, string entityKey) => $"{entityTypeKey.Trim()}::{entityKey.Trim()}";

            var entityTypeKeyById = entityTypes.Values
                .GroupBy(t => t.EntityTypeId)
                .ToDictionary(g => g.Key, g => g.First().Key);

            var recordsByKey = existingRecords
                .Where(r => !string.IsNullOrWhiteSpace(r.Key))
                .GroupBy(r => r.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var recordsByScopeKey = existingRecords
                .Where(r => !string.IsNullOrWhiteSpace(r.Key) && entityTypeKeyById.ContainsKey(r.EntityTypeId))
                .GroupBy(r => BuildRecordScopeKey(entityTypeKeyById[r.EntityTypeId], r.Key), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var importedByScopeKey = new Dictionary<string, Models.Entity>(StringComparer.OrdinalIgnoreCase);
            var importedByKey = new Dictionary<string, Models.Entity>(StringComparer.OrdinalIgnoreCase);
            var sourceByScopeKey = new Dictionary<string, EntityDataRecordItem>(StringComparer.OrdinalIgnoreCase);
            var recordsToProcessValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var recordItem in package.Records)
            {
                if (string.IsNullOrWhiteSpace(recordItem?.EntityTypeKey) || string.IsNullOrWhiteSpace(recordItem.EntityKey))
                {
                    result.RecordsSkipped++;
                    result.Warnings.Add("Skipped record with missing EntityTypeKey or EntityKey.");
                    continue;
                }

                var entityTypeKey = recordItem.EntityTypeKey.Trim();
                if (!entityTypes.TryGetValue(entityTypeKey, out var entityType))
                {
                    result.RecordsSkipped++;
                    result.Warnings.Add($"EntityType key '{recordItem.EntityTypeKey}' not found for record '{recordItem.EntityKey}'.");
                    continue;
                }

                var recordKey = recordItem.EntityKey.Trim();
                var recordScopeKey = BuildRecordScopeKey(entityTypeKey, recordKey);

                if (!recordsByScopeKey.TryGetValue(recordScopeKey, out var existing))
                {
                    existing = new Models.Entity
                    {
                        SiteId = siteId,
                        ModuleId = moduleId,
                        EntityTypeId = entityType.EntityTypeId,
                        Key = recordKey,
                        Name = recordItem.Name,
                        Description = recordItem.Description,
                        Status = recordItem.Status,
                        IsEnabled = recordItem.IsEnabled,
                        IsFeatured = recordItem.IsFeatured,
                        IsPublished = recordItem.IsPublished,
                        PublishStartDate = recordItem.PublishStartDate,
                        PublishEndDate = recordItem.PublishEndDate,
                        SortOrder = recordItem.SortOrder,
                        Settings = recordItem.Settings,
                        CreatedBy = user,
                        CreatedOn = now,
                        ModifiedBy = user,
                        ModifiedOn = now
                    };
                    db.Entity.Add(existing);
                    await db.SaveChangesAsync();
                    recordsByScopeKey[recordScopeKey] = existing;
                    if (!recordsByKey.ContainsKey(recordKey))
                    {
                        recordsByKey[recordKey] = existing;
                    }
                    result.RecordsCreated++;
                    recordsToProcessValues.Add(recordScopeKey);
                }
                else if (overwriteExisting)
                {
                    existing.EntityTypeId = entityType.EntityTypeId;
                    existing.Name = recordItem.Name;
                    existing.Description = recordItem.Description;
                    existing.Status = recordItem.Status;
                    existing.IsEnabled = recordItem.IsEnabled;
                    existing.IsFeatured = recordItem.IsFeatured;
                    existing.IsPublished = recordItem.IsPublished;
                    existing.PublishStartDate = recordItem.PublishStartDate;
                    existing.PublishEndDate = recordItem.PublishEndDate;
                    existing.SortOrder = recordItem.SortOrder;
                    existing.Settings = recordItem.Settings;
                    existing.ModifiedBy = user;
                    existing.ModifiedOn = now;
                    db.Entity.Update(existing);
                    await db.SaveChangesAsync();
                    recordsByScopeKey[recordScopeKey] = existing;
                    result.RecordsUpdated++;
                    recordsToProcessValues.Add(recordScopeKey);
                }
                else
                {
                    result.RecordsSkipped++;
                }

                importedByScopeKey[recordScopeKey] = existing;
                importedByKey[recordKey] = existing;
                sourceByScopeKey[recordScopeKey] = recordItem;
            }

            foreach (var pair in importedByScopeKey)
            {
                var recordScopeKey = pair.Key;
                var entity = pair.Value;
                var source = sourceByScopeKey[recordScopeKey];
                var recordKey = source.EntityKey?.Trim() ?? entity.Key;

                if (string.IsNullOrWhiteSpace(source.ParentEntityKey))
                {
                    continue;
                }

                var parentKey = source.ParentEntityKey.Trim();
                Models.Entity parentEntity = null;
                if (!importedByKey.TryGetValue(parentKey, out parentEntity) && !recordsByKey.TryGetValue(parentKey, out parentEntity))
                {
                    result.Warnings.Add($"Parent entity key '{parentKey}' not found for record '{recordKey}'.");
                    continue;
                }

                if (entity.ParentEntityId != parentEntity.EntityId)
                {
                    entity.ParentEntityId = parentEntity.EntityId;
                    entity.ModifiedBy = user;
                    entity.ModifiedOn = now;
                    db.Entity.Update(entity);
                }
            }

            await db.SaveChangesAsync();

            var relevantTypeIds = new HashSet<int>(importedByScopeKey.Values.Select(v => v.EntityTypeId));
            var pendingTypeIds = new Queue<int>(relevantTypeIds);
            while (pendingTypeIds.Count > 0)
            {
                var typeId = pendingTypeIds.Dequeue();
                if (!entityTypeKeyById.TryGetValue(typeId, out var typeKey) || !entityTypes.TryGetValue(typeKey, out var type) || !type.ParentEntityTypeId.HasValue)
                {
                    continue;
                }

                if (relevantTypeIds.Add(type.ParentEntityTypeId.Value))
                {
                    pendingTypeIds.Enqueue(type.ParentEntityTypeId.Value);
                }
            }

            var fields = await db.EntityFields
                .Where(f => relevantTypeIds.Contains(f.EntityTypeId))
                .ToListAsync();

            var fieldsByTypeKeyAndKey = fields
                .Where(f => entityTypeKeyById.ContainsKey(f.EntityTypeId))
                .GroupBy(f => entityTypeKeyById[f.EntityTypeId], StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);

            bool TryResolveFieldLookup(string entityTypeKey, out Dictionary<string, EntityField> resolvedLookup)
            {
                resolvedLookup = null;
                if (string.IsNullOrWhiteSpace(entityTypeKey))
                {
                    return false;
                }

                var currentTypeKey = entityTypeKey.Trim();
                var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                while (!string.IsNullOrWhiteSpace(currentTypeKey) && visited.Add(currentTypeKey))
                {
                    if (fieldsByTypeKeyAndKey.TryGetValue(currentTypeKey, out resolvedLookup))
                    {
                        return true;
                    }

                    if (!entityTypes.TryGetValue(currentTypeKey, out var currentType) || !currentType.ParentEntityTypeId.HasValue)
                    {
                        break;
                    }

                    if (!entityTypeKeyById.TryGetValue(currentType.ParentEntityTypeId.Value, out var parentTypeKey))
                    {
                        break;
                    }

                    currentTypeKey = parentTypeKey;
                }

                return false;
            }

            foreach (var pair in importedByScopeKey)
            {
                var recordScopeKey = pair.Key;
                var entity = pair.Value;
                var source = sourceByScopeKey[recordScopeKey];
                var recordKey = source.EntityKey?.Trim() ?? entity.Key;

                if (!recordsToProcessValues.Contains(recordScopeKey))
                {
                    continue;
                }

                if (overwriteExisting)
                {
                    var existingValues = await db.EntityValues.Where(v => v.EntityId == entity.EntityId).ToListAsync();
                    if (existingValues.Count > 0)
                    {
                        db.EntityValues.RemoveRange(existingValues);
                        await db.SaveChangesAsync();
                        result.ValuesUpdated += existingValues.Count;
                    }
                }

                var sourceEntityTypeKey = source.EntityTypeKey?.Trim();
                if (!TryResolveFieldLookup(sourceEntityTypeKey, out var fieldLookup))
                {
                    if (source.Values.Any())
                    {
                        result.Warnings.Add($"No fields found for entity type key '{source.EntityTypeKey}' (id '{entity.EntityTypeId}') while importing record '{recordKey}'.");
                    }
                    continue;
                }

                foreach (var valueItem in source.Values)
                {
                    if (string.IsNullOrWhiteSpace(valueItem?.FieldKey))
                    {
                        continue;
                    }

                    if (!fieldLookup.TryGetValue(valueItem.FieldKey.Trim(), out var field))
                    {
                        result.Warnings.Add($"Field key '{valueItem.FieldKey}' not found for record '{recordKey}'.");
                        continue;
                    }

                    int? referencedEntityId = null;
                    if (!string.IsNullOrWhiteSpace(valueItem.ReferencedEntityKey))
                    {
                        var referencedKey = valueItem.ReferencedEntityKey.Trim();
                        Models.Entity referencedEntity = null;
                        if (!importedByKey.TryGetValue(referencedKey, out referencedEntity) && !recordsByKey.TryGetValue(referencedKey, out referencedEntity))
                        {
                            result.Warnings.Add($"Referenced entity key '{referencedKey}' not found for record '{recordKey}', field '{valueItem.FieldKey}'.");
                        }
                        else
                        {
                            referencedEntityId = referencedEntity.EntityId;
                        }
                    }

                    var rewrittenTextValue = RewriteTextValueFileReferences(valueItem.TextValue);

                    db.EntityValues.Add(new EntityValue
                    {
                        EntityId = entity.EntityId,
                        FieldId = field.FieldId,
                        ValueIndex = valueItem.ValueIndex,
                        TextValue = rewrittenTextValue,
                        IntegerValue = valueItem.IntegerValue,
                        LongValue = valueItem.LongValue,
                        DecimalValue = valueItem.DecimalValue,
                        BooleanValue = valueItem.BooleanValue,
                        DateValue = valueItem.DateValue,
                        DateTimeValue = valueItem.DateTimeValue,
                        GuidValue = valueItem.GuidValue,
                        ReferencedEntityId = referencedEntityId,
                        CreatedBy = user,
                        CreatedOn = now,
                        ModifiedBy = user,
                        ModifiedOn = now
                    });
                    result.ValuesCreated++;
                }

                await db.SaveChangesAsync();
            }

            if (_missingZipFileEntryIds.Count > 0)
            {
                foreach (var missingFileId in _missingZipFileEntryIds.OrderBy(x => x))
                {
                    result.Warnings.Add($"Referenced fileId '{missingFileId}' was not found in ZIP file entries and could not be imported.");
                }
            }

            return result;
        }

        public async Task<byte[]> ExportDataZipAsync(int siteId, int moduleId)
        {
            var package = await ExportDataAsync(siteId, moduleId);
            package ??= new EntityDataPackage();

            var sourceFileIds = new HashSet<int>();
            foreach (var record in package.Records)
            {
                foreach (var value in record.Values)
                {
                    foreach (var fileId in ExtractFileIdsFromTextValue(value.TextValue))
                    {
                        sourceFileIds.Add(fileId);
                    }
                }
            }

            using var output = new MemoryStream();
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, true))
            {
                foreach (var sourceFileId in sourceFileIds)
                {
                    var file = _fileRepository.GetFile(sourceFileId);
                    if (file == null)
                    {
                        continue;
                    }

                    var filePath = _fileRepository.GetFilePath(file);
                    if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
                    {
                        continue;
                    }

                    var entryPath = $"files/{sourceFileId}/{file.Name}";
                    var fileEntry = archive.CreateEntry(entryPath, CompressionLevel.Fastest);
                    await using (var fileEntryStream = fileEntry.Open())
                    await using (var sourceStream = System.IO.File.OpenRead(filePath))
                    {
                        await sourceStream.CopyToAsync(fileEntryStream);
                    }

                    package.Files.Add(new EntityDataFileItem
                    {
                        SourceFileId = sourceFileId,
                        EntryPath = entryPath,
                        FileName = file.Name,
                        FolderPath = file.Folder?.Path
                    });
                }

                var json = JsonSerializer.Serialize(package, new JsonSerializerOptions { WriteIndented = true });
                var bytes = Encoding.UTF8.GetBytes(json);
                var entry = archive.CreateEntry("records.json", CompressionLevel.Fastest);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(bytes, 0, bytes.Length);
            }

            return output.ToArray();
        }

        public async Task<EntityDataImportResult> ImportDataZipAsync(int siteId, int moduleId, byte[] zipBytes, bool overwriteExisting = true)
        {
            if (zipBytes == null || zipBytes.Length == 0)
            {
                return new EntityDataImportResult { Warnings = new List<string> { "ZIP payload is empty." } };
            }

            using var stream = new MemoryStream(zipBytes);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, false);
            var recordsEntry = archive.GetEntry("records.json");
            if (recordsEntry == null)
            {
                return new EntityDataImportResult { Warnings = new List<string> { "records.json not found in ZIP package." } };
            }

            await using var entryStream = recordsEntry.Open();
            var package = await JsonSerializer.DeserializeAsync<EntityDataPackage>(entryStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (package == null)
            {
                return new EntityDataImportResult { Warnings = new List<string> { "Invalid records.json payload." } };
            }

            _zipFileSourceIds = package.Files?
                .Where(f => f != null && f.SourceFileId > 0)
                .Select(f => f.SourceFileId)
                .ToHashSet() ?? new HashSet<int>();
            _missingZipFileEntryIds = new HashSet<int>();

            _importedFilesBySourceId = await ImportFilesFromArchiveAsync(siteId, archive, package.Files);
            try
            {
                return await ImportDataAsync(siteId, moduleId, package, overwriteExisting);
            }
            finally
            {
                _importedFilesBySourceId = new Dictionary<int, Oqtane.Models.File>();
                _zipFileSourceIds = new HashSet<int>();
                _missingZipFileEntryIds = new HashSet<int>();
            }
        }

        private async Task<Dictionary<int, Oqtane.Models.File>> ImportFilesFromArchiveAsync(int siteId, ZipArchive archive, List<EntityDataFileItem> files)
        {
            var map = new Dictionary<int, Oqtane.Models.File>();
            if (files == null || files.Count == 0)
            {
                return map;
            }

            foreach (var fileItem in files)
            {
                if (fileItem == null || fileItem.SourceFileId <= 0 || string.IsNullOrWhiteSpace(fileItem.EntryPath))
                {
                    continue;
                }

                var archiveEntry = archive.GetEntry(fileItem.EntryPath);
                if (archiveEntry == null)
                {
                    _missingZipFileEntryIds.Add(fileItem.SourceFileId);
                    continue;
                }

                var folder = EnsureFolderPath(siteId, fileItem.FolderPath);
                if (folder == null)
                {
                    continue;
                }

                var fileName = string.IsNullOrWhiteSpace(fileItem.FileName) ? Path.GetFileName(fileItem.EntryPath) : fileItem.FileName;
                fileName = EnsureUniqueFileName(folder.FolderId, fileName);

                var newFile = new Oqtane.Models.File
                {
                    FolderId = folder.FolderId,
                    Name = fileName,
                    Extension = Path.GetExtension(fileName)?.TrimStart('.').ToLowerInvariant() ?? string.Empty,
                    Description = string.Empty,
                    Size = (int)archiveEntry.Length,
                    ImageHeight = 0,
                    ImageWidth = 0,
                    CreatedBy = _accessor.HttpContext?.User?.Identity?.Name ?? "system",
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = _accessor.HttpContext?.User?.Identity?.Name ?? "system",
                    ModifiedOn = DateTime.UtcNow
                };

                var filePath = _fileRepository.GetFilePath(newFile);
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    continue;
                }

                var targetDirectory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(targetDirectory) && !Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                await using (var source = archiveEntry.Open())
                await using (var destination = System.IO.File.Create(filePath))
                {
                    await source.CopyToAsync(destination);
                }

                var inserted = _fileRepository.AddFile(newFile);
                if (inserted != null)
                {
                    map[fileItem.SourceFileId] = inserted;
                }
            }

            return map;
        }

        private Folder EnsureFolderPath(int siteId, string folderPath)
        {
            var normalizedPath = (folderPath ?? string.Empty).Replace("\\", "/").Trim('/');
            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return null;
            }

            Folder currentParent = null;
            var currentPath = string.Empty;
            foreach (var segment in normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                currentPath = string.IsNullOrEmpty(currentPath) ? segment : $"{currentPath}/{segment}";
                var candidatePath = currentPath.EndsWith("/") ? currentPath : currentPath + "/";

                var folder = _folderRepository.GetFolder(siteId, candidatePath);
                if (folder == null)
                {
                    var newFolder = new Folder
                    {
                        SiteId = siteId,
                        ParentId = currentParent?.FolderId,
                        Name = segment,
                        Type = currentParent?.Type ?? FolderTypes.Public,
                        Path = candidatePath,
                        Order = 0,
                        ImageSizes = currentParent?.ImageSizes,
                        Capacity = currentParent?.Capacity ?? 0,
                        CacheControl = currentParent?.CacheControl,
                        IsSystem = false,
                        PermissionList = currentParent?.PermissionList ?? new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Everyone, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        }
                    };

                    try
                    {
                        folder = _folderRepository.AddFolder(newFolder);
                    }
                    catch (DbUpdateException)
                    {
                        folder = _folderRepository.GetFolder(siteId, candidatePath);
                    }
                }

                if (folder == null)
                {
                    return currentParent;
                }

                currentParent = folder;
            }

            return currentParent;
        }

        private string EnsureUniqueFileName(int folderId, string fileName)
        {
            var safeName = string.IsNullOrWhiteSpace(fileName) ? $"file-{Guid.NewGuid():N}.bin" : fileName;
            var extension = Path.GetExtension(safeName);
            var baseName = Path.GetFileNameWithoutExtension(safeName);
            var candidate = safeName;
            var counter = 1;

            while (_fileRepository.GetFile(folderId, candidate) != null)
            {
                candidate = $"{baseName}-{counter}{extension}";
                counter++;
            }

            return candidate;
        }

        private IEnumerable<int> ExtractFileIdsFromTextValue(string textValue)
        {
            if (string.IsNullOrWhiteSpace(textValue))
            {
                yield break;
            }

            JsonNode node;
            try
            {
                node = JsonNode.Parse(textValue);
            }
            catch
            {
                yield break;
            }

            foreach (var id in ExtractFileIdsFromNode(node))
            {
                yield return id;
            }
        }

        private IEnumerable<int> ExtractFileIdsFromNode(JsonNode node)
        {
            if (node is JsonObject obj)
            {
                if (obj.TryGetPropertyValue("fileId", out var fileIdNode) && int.TryParse(fileIdNode?.ToString(), out var fileId) && fileId > 0)
                {
                    yield return fileId;
                }

                foreach (var kv in obj)
                {
                    foreach (var childId in ExtractFileIdsFromNode(kv.Value))
                    {
                        yield return childId;
                    }
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    foreach (var childId in ExtractFileIdsFromNode(item))
                    {
                        yield return childId;
                    }
                }
            }
        }

        private string RewriteTextValueFileReferences(string textValue)
        {
            if (string.IsNullOrWhiteSpace(textValue))
            {
                return textValue;
            }

            JsonNode node;
            try
            {
                node = JsonNode.Parse(textValue);
            }
            catch
            {
                return textValue;
            }

            var changed = RewriteFileReferencesInNode(node);
            return changed ? node.ToJsonString() : textValue;
        }

        private bool RewriteFileReferencesInNode(JsonNode node)
        {
            var changed = false;

            if (node is JsonObject obj)
            {
                if (obj.TryGetPropertyValue("fileId", out var fileIdNode) && int.TryParse(fileIdNode?.ToString(), out var sourceFileId) && sourceFileId > 0)
                {
                    if (_importedFilesBySourceId.TryGetValue(sourceFileId, out var importedFile))
                    {
                        obj["fileId"] = importedFile.FileId;
                        obj["filePath"] = importedFile.Url;
                        changed = true;
                    }
                    else if (!_zipFileSourceIds.Contains(sourceFileId) || _missingZipFileEntryIds.Contains(sourceFileId))
                    {
                        _missingZipFileEntryIds.Add(sourceFileId);
                    }
                }

                foreach (var kv in obj.ToList())
                {
                    if (RewriteFileReferencesInNode(kv.Value))
                    {
                        changed = true;
                    }
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var item in arr)
                {
                    if (RewriteFileReferencesInNode(item))
                    {
                        changed = true;
                    }
                }
            }

            return changed;
        }
    }
}
