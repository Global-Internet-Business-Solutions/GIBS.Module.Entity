using System;
using System.Collections.Generic;

namespace GIBS.Module.Entity.Models
{
    public class EntityDataPackage
    {
        public DateTime ExportedOnUtc { get; set; } = DateTime.UtcNow;
        public List<EntityDataRecordItem> Records { get; set; } = new();
    }

    public class EntityDataRecordItem
    {
        public string EntityTypeKey { get; set; }
        public string EntityKey { get; set; }
        public string ParentEntityKey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishStartDate { get; set; }
        public DateTime? PublishEndDate { get; set; }
        public int SortOrder { get; set; }
        public string Settings { get; set; }
        public List<EntityDataValueItem> Values { get; set; } = new();
    }

    public class EntityDataValueItem
    {
        public string FieldKey { get; set; }
        public int ValueIndex { get; set; }
        public string TextValue { get; set; }
        public int? IntegerValue { get; set; }
        public long? LongValue { get; set; }
        public decimal? DecimalValue { get; set; }
        public bool? BooleanValue { get; set; }
        public DateTime? DateValue { get; set; }
        public DateTime? DateTimeValue { get; set; }
        public Guid? GuidValue { get; set; }
        public string ReferencedEntityKey { get; set; }
    }

    public class EntityDataImportResult
    {
        public int RecordsCreated { get; set; }
        public int RecordsUpdated { get; set; }
        public int ValuesCreated { get; set; }
        public int ValuesUpdated { get; set; }
        public int RecordsSkipped { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
