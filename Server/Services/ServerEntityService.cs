using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using GIBS.Module.Entity.Enums;
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
    public class ServerEntityService : IEntityService
    {
        private readonly IEntityRepository _EntityRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly IFolderRepository _folderRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IDbContextFactory<EntityContext> _factory;
        private readonly Alias _alias;

        public ServerEntityService(IEntityRepository EntityRepository, IUserPermissions userPermissions, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor, IFolderRepository folderRepository, IFileRepository fileRepository, IDbContextFactory<EntityContext> factory)
        {
            _EntityRepository = EntityRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
            _factory = factory;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<Models.Entity>> GetEntitysAsync(int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_EntityRepository.GetEntitys(ModuleId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Get Attempt {ModuleId}", ModuleId);
                return null;
            }
        }

        public Task<Models.Entity> GetEntityAsync(int EntityId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_EntityRepository.GetEntity(EntityId));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Get Attempt {EntityId} {ModuleId}", EntityId, ModuleId);
                return null;
            }
        }

        public Task<Models.Entity> AddEntityAsync(Models.Entity Entity)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, Entity.ModuleId, PermissionNames.Edit))
            {
                Entity = _EntityRepository.AddEntity(Entity);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Entity Added {Entity}", Entity);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Add Attempt {Entity}", Entity);
                Entity = null;
            }
            return Task.FromResult(Entity);
        }

        public Task<Models.Entity> UpdateEntityAsync(Models.Entity Entity)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, Entity.ModuleId, PermissionNames.Edit))
            {
                Entity = _EntityRepository.UpdateEntity(Entity);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Entity Updated {Entity}", Entity);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Update Attempt {Entity}", Entity);
                Entity = null;
            }
            return Task.FromResult(Entity);
        }

        public Task DeleteEntityAsync(int EntityId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                var associatedFileIds = GetAssociatedFileIds(EntityId);
                DeleteAssociatedFiles(associatedFileIds, EntityId);

                _EntityRepository.DeleteEntity(EntityId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Entity Deleted {EntityId}", EntityId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Delete Attempt {EntityId} {ModuleId}", EntityId, ModuleId);
            }
            return Task.CompletedTask;
        }

        public Task<int> EnsureEntityUploadFolderAsync(int moduleId, int baseFolderId, string entityTypeKey, string entityKeyOrName)
        {
            if (!_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EnsureEntityUploadFolder Attempt {ModuleId}", moduleId);
                return Task.FromResult(baseFolderId);
            }

            var baseFolder = _folderRepository.GetFolder(baseFolderId);
            if (baseFolder == null || baseFolder.SiteId != _alias.SiteId)
            {
                return Task.FromResult(baseFolderId);
            }

            var typeSegment = SanitizeFolderSegment(entityTypeKey);
            var entitySegment = SanitizeFolderSegment(entityKeyOrName);

            var typeFolder = EnsureChildFolder(baseFolder, typeSegment);
            if (typeFolder == null)
            {
                return Task.FromResult(baseFolderId);
            }

            var entityFolder = EnsureChildFolder(typeFolder, entitySegment);
            return Task.FromResult(entityFolder?.FolderId ?? typeFolder.FolderId);
        }

        public Task<int> MoveFileToFolderAsync(int moduleId, int fileId, int targetFolderId)
        {
            if (!_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized MoveFileToFolder Attempt {ModuleId} {FileId}", moduleId, fileId);
                return Task.FromResult(fileId);
            }

            var file = _fileRepository.GetFile(fileId, true);
            var targetFolder = _folderRepository.GetFolder(targetFolderId);

            if (file == null || targetFolder == null || targetFolder.SiteId != _alias.SiteId)
            {
                return Task.FromResult(fileId);
            }

            if (file.FolderId == targetFolderId)
            {
                return Task.FromResult(fileId);
            }

            var sourcePath = _fileRepository.GetFilePath(file);
            var destinationDirectory = _folderRepository.GetFolderPath(targetFolder);
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            var destinationPath = Path.Combine(destinationDirectory, file.Name);
            if (!string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(sourcePath))
            {
                if (System.IO.File.Exists(destinationPath))
                {
                    System.IO.File.Delete(destinationPath);
                }
                System.IO.File.Move(sourcePath, destinationPath);
            }

            file.FolderId = targetFolderId;
            file.Folder = null;
            _fileRepository.UpdateFile(file);

            return Task.FromResult(fileId);
        }

        private HashSet<int> GetAssociatedFileIds(int entityId)
        {
            var fileIds = new HashSet<int>();

            using var db = _factory.CreateDbContext();
            var fileValueTexts = (from value in db.EntityValues
                                  join field in db.EntityFields on value.FieldId equals field.FieldId
                                  where value.EntityId == entityId
                                        && (field.EditorType == EntityEditorType.FileUpload || field.EditorType == EntityEditorType.ImageUpload)
                                        && !string.IsNullOrWhiteSpace(value.TextValue)
                                  select value.TextValue)
                                  .AsNoTracking()
                                  .ToList();

            foreach (var textValue in fileValueTexts)
            {
                foreach (var fileId in ExtractFileIdsFromTextValue(textValue))
                {
                    fileIds.Add(fileId);
                }
            }

            return fileIds;
        }

        private void DeleteAssociatedFiles(IEnumerable<int> fileIds, int entityId)
        {
            foreach (var fileId in fileIds.Distinct())
            {
                try
                {
                    var file = _fileRepository.GetFile(fileId, true);
                    if (file?.Folder == null || file.Folder.SiteId != _alias.SiteId)
                    {
                        continue;
                    }

                    var filePath = _fileRepository.GetFilePath(file);
                    if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    _fileRepository.DeleteFile(fileId);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Delete, ex, "Error deleting associated file {FileId} for entity {EntityId}", fileId, entityId);
                }
            }
        }

        private static IEnumerable<int> ExtractFileIdsFromTextValue(string textValue)
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

            foreach (var fileId in ExtractFileIdsFromNode(node))
            {
                yield return fileId;
            }
        }

        private static IEnumerable<int> ExtractFileIdsFromNode(JsonNode node)
        {
            if (node == null)
            {
                yield break;
            }

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

        private Folder EnsureChildFolder(Folder parentFolder, string childFolderName)
        {
            if (parentFolder == null)
            {
                return null;
            }

            var normalizedPath = Utilities.UrlCombine(parentFolder.Path, childFolderName);
            if (!normalizedPath.EndsWith('/'))
            {
                normalizedPath += "/";
            }

            var existingFolder = _folderRepository.GetFolder(parentFolder.SiteId, normalizedPath)
                ?? _folderRepository.GetFolder(parentFolder.SiteId, "/" + normalizedPath.TrimStart('/'));

            if (existingFolder != null)
            {
                return existingFolder;
            }

            var folder = new Folder
            {
                SiteId = parentFolder.SiteId,
                ParentId = parentFolder.FolderId,
                Type = parentFolder.Type,
                Name = childFolderName,
                Path = normalizedPath,
                Order = 0,
                ImageSizes = parentFolder.ImageSizes,
                Capacity = parentFolder.Capacity,
                IsSystem = false,
                CacheControl = parentFolder.CacheControl,
                PermissionList = parentFolder.PermissionList
            };

            try
            {
                return _folderRepository.AddFolder(folder);
            }
            catch (DbUpdateException)
            {
                // Duplicate-key race: folder was created concurrently.
                return _folderRepository.GetFolder(parentFolder.SiteId, normalizedPath)
                    ?? _folderRepository.GetFolder(parentFolder.SiteId, "/" + normalizedPath.TrimStart('/'));
            }
        }

        private static string SanitizeFolderSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "unassigned";
            }

            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var cleaned = new string(value.Trim().Select(c => invalidChars.Contains(c) ? '-' : c).ToArray());
            cleaned = cleaned.Replace('/', '-').Replace('\\', '-');

            return string.IsNullOrWhiteSpace(cleaned) ? "unassigned" : cleaned;
        }
    }
}
