using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;
using GIBS.Module.Entity.Models;
using GIBS.Module.Entity.Repository;

namespace GIBS.Module.Entity.Services
{
    public class ServerEntitySchemaPortabilityService : IEntitySchemaPortabilityService
    {
        private readonly IEntityTypeRepository _entityTypeRepository;
        private readonly IEntityFieldGroupRepository _fieldGroupRepository;
        private readonly IEntityFieldRepository _fieldRepository;
        private readonly IEntityFieldOptionRepository _fieldOptionRepository;
        private readonly IEntityTemplateRepository _templateRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerEntitySchemaPortabilityService(
            IEntityTypeRepository entityTypeRepository,
            IEntityFieldGroupRepository fieldGroupRepository,
            IEntityFieldRepository fieldRepository,
            IEntityFieldOptionRepository fieldOptionRepository,
            IEntityTemplateRepository templateRepository,
            IUserPermissions userPermissions,
            ITenantManager tenantManager,
            ILogManager logger,
            IHttpContextAccessor accessor)
        {
            _entityTypeRepository = entityTypeRepository;
            _fieldGroupRepository = fieldGroupRepository;
            _fieldRepository = fieldRepository;
            _fieldOptionRepository = fieldOptionRepository;
            _templateRepository = templateRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<EntitySchemaPackage> ExportSchemaAsync(int siteId, int moduleId)
        {
            if (!_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntitySchema export attempt {SiteId} {ModuleId}", siteId, moduleId);
                return Task.FromResult<EntitySchemaPackage>(null);
            }

            var package = new EntitySchemaPackage();
            var entityTypes = _entityTypeRepository.GetEntityTypes(siteId, moduleId).ToList();
            var entityTypesById = entityTypes.ToDictionary(x => x.EntityTypeId, x => x);

            var defaultTemplates = _templateRepository.GetTemplates(moduleId)
                .Where(t => !t.EntityTypeId.HasValue)
                .OrderBy(t => t.TemplateType)
                .ToList();

            foreach (var template in defaultTemplates)
            {
                package.DefaultTemplates.Add(new EntityTemplateSchemaItem
                {
                    TemplateType = template.TemplateType,
                    Header = template.Header,
                    Item = template.Item,
                    Alternate = template.Alternate,
                    Separator = template.Separator,
                    Footer = template.Footer,
                    PageTitle = template.PageTitle,
                    PageDescription = template.PageDescription,
                    PageKeywords = template.PageKeywords,
                    PageHeader = template.PageHeader
                });
            }

            var typeTemplates = _templateRepository.GetTemplates(moduleId)
                .Where(t => t.EntityTypeId.HasValue)
                .OrderBy(t => t.TemplateType)
                .ToList();

            foreach (var template in typeTemplates)
            {
                if (!entityTypesById.TryGetValue(template.EntityTypeId.GetValueOrDefault(), out var mappedEntityType))
                {
                    continue;
                }

                package.TypeTemplates.Add(new EntityTypeTemplateSchemaItem
                {
                    EntityTypeKey = mappedEntityType.Key,
                    TemplateType = template.TemplateType,
                    Header = template.Header,
                    Item = template.Item,
                    Alternate = template.Alternate,
                    Separator = template.Separator,
                    Footer = template.Footer,
                    PageTitle = template.PageTitle,
                    PageDescription = template.PageDescription,
                    PageKeywords = template.PageKeywords,
                    PageHeader = template.PageHeader
                });
            }

            foreach (var entityType in entityTypes)
            {
                var typeItem = new EntityTypeSchemaItem
                {
                    Key = entityType.Key,
                    Name = entityType.Name,
                    Description = entityType.Description,
                    ParentKey = entityType.ParentEntityTypeId.HasValue && entityTypesById.TryGetValue(entityType.ParentEntityTypeId.Value, out var parentType)
                        ? parentType.Key
                        : null,
                    SortOrder = entityType.SortOrder,
                    IsEnabled = entityType.IsEnabled,
                    IsSystem = entityType.IsSystem
                };

                var groups = _fieldGroupRepository.GetFieldGroups(entityType.EntityTypeId)
                    .OrderBy(g => g.SortOrder)
                    .ThenBy(g => g.Name)
                    .ToList();

                foreach (var group in groups)
                {
                    typeItem.FieldGroups.Add(new EntityFieldGroupSchemaItem
                    {
                        Key = group.Key,
                        Name = group.Name,
                        Description = group.Description,
                        SortOrder = group.SortOrder,
                        IsEnabled = group.IsEnabled
                    });
                }

                var fields = _fieldRepository.GetFields(entityType.EntityTypeId)
                    .OrderBy(f => f.SortOrder)
                    .ThenBy(f => f.Name)
                    .ToList();

                var groupsById = groups.ToDictionary(g => g.FieldGroupId, g => g.Key);
                foreach (var field in fields)
                {
                    var fieldItem = new EntityFieldSchemaItem
                    {
                        Key = field.Key,
                        Name = field.Name,
                        Label = field.Label,
                        Description = field.Description,
                        DataType = (int)field.DataType,
                        EditorType = (int)field.EditorType,
                        FieldGroupKey = field.FieldGroupId.HasValue && groupsById.TryGetValue(field.FieldGroupId.Value, out var groupKey) ? groupKey : null,
                        SortOrder = field.SortOrder,
                        IsEnabled = field.IsEnabled,
                        IsRequired = field.IsRequired,
                        IsReadOnly = field.IsReadOnly,
                        IsHidden = field.IsHidden,
                        IsMultiValue = field.IsMultiValue,
                        IsSearchable = field.IsSearchable,
                        IsFilterable = field.IsFilterable,
                        IsSortable = field.IsSortable,
                        IsListed = field.IsListed,
                        IsManagerOnly = field.IsManagerOnly,
                        IsCaptionHidden = field.IsCaptionHidden,
                        IsLockedDown = field.IsLockedDown,
                        ReferencedEntityTypeKey = field.ReferencedEntityTypeId.HasValue && entityTypesById.TryGetValue(field.ReferencedEntityTypeId.Value, out var refType)
                            ? refType.Key
                            : null,
                        DefaultValue = field.DefaultValue,
                        Placeholder = field.Placeholder,
                        HelpText = field.HelpText,
                        HtmlContent = field.HtmlContent,
                        ValidationSettings = field.ValidationSettings,
                        EditorSettings = field.EditorSettings,
                        MaxLength = field.MaxLength,
                        Precision = field.Precision,
                        Scale = field.Scale,
                        MinimumValues = field.MinimumValues,
                        MaximumValues = field.MaximumValues
                    };

                    var fieldOptions = _fieldOptionRepository.GetFieldOptions(field.FieldId)
                        .OrderBy(o => o.SortOrder)
                        .ThenBy(o => o.DisplayText)
                        .ToList();
                    foreach (var option in fieldOptions)
                    {
                        fieldItem.FieldOptions.Add(new EntityFieldOptionSchemaItem
                        {
                            Key = option.Key,
                            Value = option.Value,
                            DisplayText = option.DisplayText,
                            SortOrder = option.SortOrder,
                            IsDefault = option.IsDefault,
                            IsEnabled = option.IsEnabled
                        });
                    }

                    typeItem.Fields.Add(fieldItem);
                }

                package.EntityTypes.Add(typeItem);
            }

            return Task.FromResult(package);
        }

        public Task<EntitySchemaImportResult> ImportSchemaAsync(int siteId, int moduleId, EntitySchemaPackage package, bool overwriteExisting = true)
        {
            var result = new EntitySchemaImportResult();

            if (!_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntitySchema import attempt {SiteId} {ModuleId}", siteId, moduleId);
                result.Warnings.Add("Unauthorized import attempt.");
                return Task.FromResult(result);
            }

            if (package?.EntityTypes == null || package.EntityTypes.Count == 0)
            {
                result.Warnings.Add("Import package is empty.");
                return Task.FromResult(result);
            }

            var now = DateTime.UtcNow;
            var user = _accessor.HttpContext?.User?.Identity?.Name ?? "system";
            var importedTypeByKey = new Dictionary<string, Models.EntityType>(StringComparer.OrdinalIgnoreCase);

            foreach (var typeItem in package.EntityTypes)
            {
                if (string.IsNullOrWhiteSpace(typeItem?.Key))
                {
                    result.Warnings.Add("Skipped EntityType with missing key.");
                    continue;
                }

                var existingType = _entityTypeRepository.GetEntityTypeByKey(siteId, moduleId, typeItem.Key.Trim());
                if (existingType == null)
                {
                    var newType = new Models.EntityType
                    {
                        SiteId = siteId,
                        ModuleId = moduleId,
                        Key = typeItem.Key.Trim(),
                        Name = typeItem.Name,
                        Description = typeItem.Description,
                        SortOrder = typeItem.SortOrder,
                        IsEnabled = typeItem.IsEnabled,
                        IsSystem = typeItem.IsSystem,
                        CreatedBy = user,
                        CreatedOn = now,
                        ModifiedBy = user,
                        ModifiedOn = now
                    };

                    existingType = _entityTypeRepository.AddEntityType(newType);
                    result.EntityTypesCreated++;
                }
                else if (overwriteExisting)
                {
                    existingType.Name = typeItem.Name;
                    existingType.Description = typeItem.Description;
                    existingType.SortOrder = typeItem.SortOrder;
                    existingType.IsEnabled = typeItem.IsEnabled;
                    existingType.IsSystem = typeItem.IsSystem;
                    existingType.ModifiedBy = user;
                    existingType.ModifiedOn = now;
                    existingType = _entityTypeRepository.UpdateEntityType(existingType);
                    result.EntityTypesUpdated++;
                }

                importedTypeByKey[typeItem.Key.Trim()] = existingType;
            }

            foreach (var typeItem in package.EntityTypes.Where(t => !string.IsNullOrWhiteSpace(t?.Key) && !string.IsNullOrWhiteSpace(t.ParentKey)))
            {
                if (!importedTypeByKey.TryGetValue(typeItem.Key.Trim(), out var typeEntity))
                {
                    continue;
                }

                if (importedTypeByKey.TryGetValue(typeItem.ParentKey.Trim(), out var parentEntity))
                {
                    if (typeEntity.ParentEntityTypeId != parentEntity.EntityTypeId)
                    {
                        typeEntity.ParentEntityTypeId = parentEntity.EntityTypeId;
                        typeEntity.ModifiedBy = user;
                        typeEntity.ModifiedOn = now;
                        _entityTypeRepository.UpdateEntityType(typeEntity);
                    }
                }
                else
                {
                    result.Warnings.Add($"Parent key '{typeItem.ParentKey}' not found for EntityType '{typeItem.Key}'.");
                }
            }

            foreach (var typeItem in package.EntityTypes.Where(t => !string.IsNullOrWhiteSpace(t?.Key)))
            {
                if (!importedTypeByKey.TryGetValue(typeItem.Key.Trim(), out var typeEntity))
                {
                    continue;
                }

                var groupByKey = new Dictionary<string, Models.EntityFieldGroup>(StringComparer.OrdinalIgnoreCase);
                foreach (var groupItem in typeItem.FieldGroups ?? new List<EntityFieldGroupSchemaItem>())
                {
                    if (string.IsNullOrWhiteSpace(groupItem?.Key))
                    {
                        result.Warnings.Add($"Skipped FieldGroup with missing key in EntityType '{typeItem.Key}'.");
                        continue;
                    }

                    var existingGroup = _fieldGroupRepository.GetFieldGroupByKey(typeEntity.EntityTypeId, groupItem.Key.Trim());
                    if (existingGroup == null)
                    {
                        existingGroup = _fieldGroupRepository.AddFieldGroup(new Models.EntityFieldGroup
                        {
                            EntityTypeId = typeEntity.EntityTypeId,
                            Key = groupItem.Key.Trim(),
                            Name = groupItem.Name,
                            Description = groupItem.Description,
                            SortOrder = groupItem.SortOrder,
                            IsEnabled = groupItem.IsEnabled,
                            CreatedBy = user,
                            CreatedOn = now,
                            ModifiedBy = user,
                            ModifiedOn = now
                        });
                        result.FieldGroupsCreated++;
                    }
                    else if (overwriteExisting)
                    {
                        existingGroup.Name = groupItem.Name;
                        existingGroup.Description = groupItem.Description;
                        existingGroup.SortOrder = groupItem.SortOrder;
                        existingGroup.IsEnabled = groupItem.IsEnabled;
                        existingGroup.ModifiedBy = user;
                        existingGroup.ModifiedOn = now;
                        existingGroup = _fieldGroupRepository.UpdateFieldGroup(existingGroup);
                        result.FieldGroupsUpdated++;
                    }

                    groupByKey[groupItem.Key.Trim()] = existingGroup;
                }

                foreach (var fieldItem in typeItem.Fields ?? new List<EntityFieldSchemaItem>())
                {
                    if (string.IsNullOrWhiteSpace(fieldItem?.Key))
                    {
                        result.Warnings.Add($"Skipped Field with missing key in EntityType '{typeItem.Key}'.");
                        continue;
                    }

                    int? fieldGroupId = null;
                    if (!string.IsNullOrWhiteSpace(fieldItem.FieldGroupKey))
                    {
                        if (groupByKey.TryGetValue(fieldItem.FieldGroupKey.Trim(), out var mappedGroup))
                        {
                            fieldGroupId = mappedGroup.FieldGroupId;
                        }
                        else
                        {
                            result.Warnings.Add($"Field group key '{fieldItem.FieldGroupKey}' not found for Field '{fieldItem.Key}' in EntityType '{typeItem.Key}'.");
                        }
                    }

                    int? referencedEntityTypeId = null;
                    if (!string.IsNullOrWhiteSpace(fieldItem.ReferencedEntityTypeKey))
                    {
                        if (importedTypeByKey.TryGetValue(fieldItem.ReferencedEntityTypeKey.Trim(), out var referencedType))
                        {
                            referencedEntityTypeId = referencedType.EntityTypeId;
                        }
                        else
                        {
                            result.Warnings.Add($"Referenced EntityType key '{fieldItem.ReferencedEntityTypeKey}' not found for Field '{fieldItem.Key}' in EntityType '{typeItem.Key}'.");
                        }
                    }

                    var existingField = _fieldRepository.GetFieldByKey(typeEntity.EntityTypeId, fieldItem.Key.Trim());
                    if (existingField == null)
                    {
                        existingField = _fieldRepository.AddField(new Models.EntityField
                        {
                            EntityTypeId = typeEntity.EntityTypeId,
                            FieldGroupId = fieldGroupId,
                            Key = fieldItem.Key.Trim(),
                            Name = fieldItem.Name,
                            Label = fieldItem.Label,
                            Description = fieldItem.Description,
                            DataType = (GIBS.Module.Entity.Enums.EntityDataType)fieldItem.DataType,
                            EditorType = (GIBS.Module.Entity.Enums.EntityEditorType)fieldItem.EditorType,
                            SortOrder = fieldItem.SortOrder,
                            IsEnabled = fieldItem.IsEnabled,
                            IsRequired = fieldItem.IsRequired,
                            IsReadOnly = fieldItem.IsReadOnly,
                            IsHidden = fieldItem.IsHidden,
                            IsMultiValue = fieldItem.IsMultiValue,
                            IsSearchable = fieldItem.IsSearchable,
                            IsFilterable = fieldItem.IsFilterable,
                            IsSortable = fieldItem.IsSortable,
                            IsListed = fieldItem.IsListed,
                            IsManagerOnly = fieldItem.IsManagerOnly,
                            IsCaptionHidden = fieldItem.IsCaptionHidden,
                            IsLockedDown = fieldItem.IsLockedDown,
                            ReferencedEntityTypeId = referencedEntityTypeId,
                            DefaultValue = fieldItem.DefaultValue,
                            Placeholder = fieldItem.Placeholder,
                            HelpText = fieldItem.HelpText,
                            HtmlContent = fieldItem.HtmlContent,
                            ValidationSettings = fieldItem.ValidationSettings,
                            EditorSettings = fieldItem.EditorSettings,
                            MaxLength = fieldItem.MaxLength,
                            Precision = fieldItem.Precision,
                            Scale = fieldItem.Scale,
                            MinimumValues = fieldItem.MinimumValues,
                            MaximumValues = fieldItem.MaximumValues,
                            CreatedBy = user,
                            CreatedOn = now,
                            ModifiedBy = user,
                            ModifiedOn = now
                        });
                        result.FieldsCreated++;
                    }
                    else if (overwriteExisting)
                    {
                        existingField.FieldGroupId = fieldGroupId;
                        existingField.Name = fieldItem.Name;
                        existingField.Label = fieldItem.Label;
                        existingField.Description = fieldItem.Description;
                        existingField.DataType = (GIBS.Module.Entity.Enums.EntityDataType)fieldItem.DataType;
                        existingField.EditorType = (GIBS.Module.Entity.Enums.EntityEditorType)fieldItem.EditorType;
                        existingField.SortOrder = fieldItem.SortOrder;
                        existingField.IsEnabled = fieldItem.IsEnabled;
                        existingField.IsRequired = fieldItem.IsRequired;
                        existingField.IsReadOnly = fieldItem.IsReadOnly;
                        existingField.IsHidden = fieldItem.IsHidden;
                        existingField.IsMultiValue = fieldItem.IsMultiValue;
                        existingField.IsSearchable = fieldItem.IsSearchable;
                        existingField.IsFilterable = fieldItem.IsFilterable;
                        existingField.IsSortable = fieldItem.IsSortable;
                        existingField.IsListed = fieldItem.IsListed;
                        existingField.IsManagerOnly = fieldItem.IsManagerOnly;
                        existingField.IsCaptionHidden = fieldItem.IsCaptionHidden;
                        existingField.IsLockedDown = fieldItem.IsLockedDown;
                        existingField.ReferencedEntityTypeId = referencedEntityTypeId;
                        existingField.DefaultValue = fieldItem.DefaultValue;
                        existingField.Placeholder = fieldItem.Placeholder;
                        existingField.HelpText = fieldItem.HelpText;
                        existingField.HtmlContent = fieldItem.HtmlContent;
                        existingField.ValidationSettings = fieldItem.ValidationSettings;
                        existingField.EditorSettings = fieldItem.EditorSettings;
                        existingField.MaxLength = fieldItem.MaxLength;
                        existingField.Precision = fieldItem.Precision;
                        existingField.Scale = fieldItem.Scale;
                        existingField.MinimumValues = fieldItem.MinimumValues;
                        existingField.MaximumValues = fieldItem.MaximumValues;
                        existingField.ModifiedBy = user;
                        existingField.ModifiedOn = now;
                        _fieldRepository.UpdateField(existingField);
                        result.FieldsUpdated++;
                    }

                    foreach (var optionItem in fieldItem.FieldOptions ?? new List<EntityFieldOptionSchemaItem>())
                    {
                        if (string.IsNullOrWhiteSpace(optionItem?.Key))
                        {
                            result.Warnings.Add($"Skipped FieldOption with missing key for Field '{fieldItem.Key}' in EntityType '{typeItem.Key}'.");
                            continue;
                        }

                        var optionKey = optionItem.Key.Trim();
                        var existingOption = _fieldOptionRepository.GetFieldOptionByKey(existingField.FieldId, optionKey);
                        if (existingOption == null)
                        {
                            _fieldOptionRepository.AddFieldOption(new Models.EntityFieldOption
                            {
                                FieldId = existingField.FieldId,
                                Key = optionKey,
                                Value = optionItem.Value,
                                DisplayText = optionItem.DisplayText,
                                SortOrder = optionItem.SortOrder,
                                IsDefault = optionItem.IsDefault,
                                IsEnabled = optionItem.IsEnabled,
                                CreatedBy = user,
                                CreatedOn = now,
                                ModifiedBy = user,
                                ModifiedOn = now
                            });
                            result.FieldOptionsCreated++;
                        }
                        else if (overwriteExisting)
                        {
                            existingOption.Value = optionItem.Value;
                            existingOption.DisplayText = optionItem.DisplayText;
                            existingOption.SortOrder = optionItem.SortOrder;
                            existingOption.IsDefault = optionItem.IsDefault;
                            existingOption.IsEnabled = optionItem.IsEnabled;
                            existingOption.ModifiedBy = user;
                            existingOption.ModifiedOn = now;
                            _fieldOptionRepository.UpdateFieldOption(existingOption);
                            result.FieldOptionsUpdated++;
                        }
                    }
                }
            }

            var existingDefaultTemplates = _templateRepository.GetTemplates(moduleId)
                .Where(t => !t.EntityTypeId.HasValue)
                .ToList();

            foreach (var templateItem in package.DefaultTemplates ?? new List<EntityTemplateSchemaItem>())
            {
                if (string.IsNullOrWhiteSpace(templateItem?.TemplateType))
                {
                    result.Warnings.Add("Skipped Default Template with missing TemplateType.");
                    continue;
                }

                var templateType = templateItem.TemplateType.Trim();
                var existingTemplate = existingDefaultTemplates.FirstOrDefault(t => string.Equals(t.TemplateType, templateType, StringComparison.OrdinalIgnoreCase));
                if (existingTemplate == null)
                {
                    _templateRepository.AddTemplate(new Models.EntityTemplate
                    {
                        ModuleId = moduleId,
                        EntityTypeId = null,
                        TemplateType = templateType,
                        Header = templateItem.Header,
                        Item = templateItem.Item,
                        Alternate = templateItem.Alternate,
                        Separator = templateItem.Separator,
                        Footer = templateItem.Footer,
                        PageTitle = templateItem.PageTitle,
                        PageDescription = templateItem.PageDescription,
                        PageKeywords = templateItem.PageKeywords,
                        PageHeader = templateItem.PageHeader,
                        CreatedBy = user,
                        CreatedOn = now,
                        ModifiedBy = user,
                        ModifiedOn = now
                    });
                    result.TemplatesCreated++;
                }
                else if (overwriteExisting)
                {
                    existingTemplate.Header = templateItem.Header;
                    existingTemplate.Item = templateItem.Item;
                    existingTemplate.Alternate = templateItem.Alternate;
                    existingTemplate.Separator = templateItem.Separator;
                    existingTemplate.Footer = templateItem.Footer;
                    existingTemplate.PageTitle = templateItem.PageTitle;
                    existingTemplate.PageDescription = templateItem.PageDescription;
                    existingTemplate.PageKeywords = templateItem.PageKeywords;
                    existingTemplate.PageHeader = templateItem.PageHeader;
                    existingTemplate.ModifiedBy = user;
                    existingTemplate.ModifiedOn = now;
                    _templateRepository.UpdateTemplate(existingTemplate);
                    result.TemplatesUpdated++;
                }
            }

            var existingTypeTemplates = _templateRepository.GetTemplates(moduleId)
                .Where(t => t.EntityTypeId.HasValue)
                .ToList();

            foreach (var templateItem in package.TypeTemplates ?? new List<EntityTypeTemplateSchemaItem>())
            {
                if (string.IsNullOrWhiteSpace(templateItem?.TemplateType) || string.IsNullOrWhiteSpace(templateItem.EntityTypeKey))
                {
                    result.Warnings.Add("Skipped EntityType Template with missing TemplateType or EntityTypeKey.");
                    continue;
                }

                var entityTypeKey = templateItem.EntityTypeKey.Trim();
                if (!importedTypeByKey.TryGetValue(entityTypeKey, out var mappedEntityType))
                {
                    result.Warnings.Add($"EntityType key '{entityTypeKey}' not found for template '{templateItem.TemplateType}'.");
                    continue;
                }

                var templateType = templateItem.TemplateType.Trim();
                var existingTypeTemplate = existingTypeTemplates.FirstOrDefault(t =>
                    t.EntityTypeId == mappedEntityType.EntityTypeId &&
                    string.Equals(t.TemplateType, templateType, StringComparison.OrdinalIgnoreCase));

                if (existingTypeTemplate == null)
                {
                    _templateRepository.AddTemplate(new Models.EntityTemplate
                    {
                        ModuleId = moduleId,
                        EntityTypeId = mappedEntityType.EntityTypeId,
                        TemplateType = templateType,
                        Header = templateItem.Header,
                        Item = templateItem.Item,
                        Alternate = templateItem.Alternate,
                        Separator = templateItem.Separator,
                        Footer = templateItem.Footer,
                        PageTitle = templateItem.PageTitle,
                        PageDescription = templateItem.PageDescription,
                        PageKeywords = templateItem.PageKeywords,
                        PageHeader = templateItem.PageHeader,
                        CreatedBy = user,
                        CreatedOn = now,
                        ModifiedBy = user,
                        ModifiedOn = now
                    });
                    result.TypeTemplatesCreated++;
                }
                else if (overwriteExisting)
                {
                    existingTypeTemplate.Header = templateItem.Header;
                    existingTypeTemplate.Item = templateItem.Item;
                    existingTypeTemplate.Alternate = templateItem.Alternate;
                    existingTypeTemplate.Separator = templateItem.Separator;
                    existingTypeTemplate.Footer = templateItem.Footer;
                    existingTypeTemplate.PageTitle = templateItem.PageTitle;
                    existingTypeTemplate.PageDescription = templateItem.PageDescription;
                    existingTypeTemplate.PageKeywords = templateItem.PageKeywords;
                    existingTypeTemplate.PageHeader = templateItem.PageHeader;
                    existingTypeTemplate.ModifiedBy = user;
                    existingTypeTemplate.ModifiedOn = now;
                    _templateRepository.UpdateTemplate(existingTypeTemplate);
                    result.TypeTemplatesUpdated++;
                }
            }

            return Task.FromResult(result);
        }
    }
}
