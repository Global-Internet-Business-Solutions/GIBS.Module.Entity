using System.Collections.Generic;

namespace GIBS.Module.Entity.Models
{
    public class EntitySchemaPackage
    {
        public List<EntityTypeSchemaItem> EntityTypes { get; set; } = new();
        public List<EntityTemplateSchemaItem> DefaultTemplates { get; set; } = new();
        public List<EntityTypeTemplateSchemaItem> TypeTemplates { get; set; } = new();
    }

    public class EntityTypeSchemaItem
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ParentKey { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsSystem { get; set; }
        public List<EntityFieldGroupSchemaItem> FieldGroups { get; set; } = new();
        public List<EntityFieldSchemaItem> Fields { get; set; } = new();
    }

    public class EntityFieldGroupSchemaItem
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class EntityFieldSchemaItem
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public int DataType { get; set; }
        public int EditorType { get; set; }
        public string FieldGroupKey { get; set; }
        public int SortOrder { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsMultiValue { get; set; }
        public bool IsSearchable { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsSortable { get; set; }
        public bool IsListed { get; set; }
        public bool IsManagerOnly { get; set; }
        public bool IsCaptionHidden { get; set; }
        public bool IsLockedDown { get; set; }
        public string ReferencedEntityTypeKey { get; set; }
        public string DefaultValue { get; set; }
        public string Placeholder { get; set; }
        public string HelpText { get; set; }
        public string HtmlContent { get; set; }
        public string ValidationSettings { get; set; }
        public string EditorSettings { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public int? MinimumValues { get; set; }
        public int? MaximumValues { get; set; }
        public List<EntityFieldOptionSchemaItem> FieldOptions { get; set; } = new();
    }

    public class EntityFieldOptionSchemaItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string DisplayText { get; set; }
        public int SortOrder { get; set; }
        public bool IsDefault { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class EntityTemplateSchemaItem
    {
        public string TemplateType { get; set; }
        public string Header { get; set; }
        public string Item { get; set; }
        public string Alternate { get; set; }
        public string Separator { get; set; }
        public string Footer { get; set; }
        public string PageTitle { get; set; }
        public string PageDescription { get; set; }
        public string PageKeywords { get; set; }
        public string PageHeader { get; set; }
    }

    public class EntityTypeTemplateSchemaItem : EntityTemplateSchemaItem
    {
        public string EntityTypeKey { get; set; }
    }

    public class EntitySchemaImportResult
    {
        public int EntityTypesCreated { get; set; }
        public int EntityTypesUpdated { get; set; }
        public int FieldGroupsCreated { get; set; }
        public int FieldGroupsUpdated { get; set; }
        public int FieldsCreated { get; set; }
        public int FieldsUpdated { get; set; }
        public int TemplatesCreated { get; set; }
        public int TemplatesUpdated { get; set; }
        public int TypeTemplatesCreated { get; set; }
        public int TypeTemplatesUpdated { get; set; }
        public int FieldOptionsCreated { get; set; }
        public int FieldOptionsUpdated { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
