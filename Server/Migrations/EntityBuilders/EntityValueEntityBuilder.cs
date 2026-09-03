using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace GIBS.Module.Entity.Migrations.EntityBuilders
{
    public class EntityValueEntityBuilder : BaseEntityBuilder<EntityValueEntityBuilder>
    {
        private const string _entityTableName = "GIBS_EntityValue";
        private readonly PrimaryKey<EntityValueEntityBuilder> _primaryKey = 
            new("PK_GIBS_EntityValue", x => x.EntityValueId);
        private readonly ForeignKey<EntityValueEntityBuilder> _entityForeignKey = 
            new("FK_GIBS_EntityValue_Entity", x => x.EntityId, "GIBS_Entity", "EntityId", ReferentialAction.Cascade);
        private readonly ForeignKey<EntityValueEntityBuilder> _fieldForeignKey = 
            new("FK_GIBS_EntityValue_EntityField", x => x.FieldId, "GIBS_EntityField", "FieldId", ReferentialAction.Restrict);
        private readonly ForeignKey<EntityValueEntityBuilder> _referencedEntityForeignKey = 
            new("FK_GIBS_EntityValue_ReferencedEntity", x => x.ReferencedEntityId, "GIBS_Entity", "EntityId", ReferentialAction.NoAction);

        public EntityValueEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) 
            : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_entityForeignKey);
            ForeignKeys.Add(_fieldForeignKey);
            ForeignKeys.Add(_referencedEntityForeignKey);
        }

        protected override EntityValueEntityBuilder BuildTable(ColumnsBuilder table)
        {
            EntityValueId = AddAutoIncrementColumn(table, "EntityValueId");
            EntityId = AddIntegerColumn(table, "EntityId");
            FieldId = AddIntegerColumn(table, "FieldId");
            ValueIndex = AddIntegerColumn(table, "ValueIndex", false, 0);

            // Typed value columns
            TextValue = AddMaxStringColumn(table, "TextValue", true);
            IntegerValue = AddIntegerColumn(table, "IntegerValue", true);
            LongValue = AddLongColumn(table, "LongValue", true);
            DecimalValue = AddDecimalColumn(table, "DecimalValue", 18, 4, true);
            BooleanValue = AddBooleanColumn(table, "BooleanValue", true);
            DateValue = AddDateTimeColumn(table, "DateValue", true);
            DateTimeValue = AddDateTimeColumn(table, "DateTimeValue", true);
            GuidValue = AddGuidColumn(table, "GuidValue", true);
            ReferencedEntityId = AddIntegerColumn(table, "ReferencedEntityId", true);

            // Audit columns (from ModelBase)
            CreatedBy = AddStringColumn(table, "CreatedBy", 256, true);
            CreatedOn = AddDateTimeColumn(table, "CreatedOn", false);
            ModifiedBy = AddStringColumn(table, "ModifiedBy", 256, true);
            ModifiedOn = AddDateTimeColumn(table, "ModifiedOn", false);

            return this;
        }

        public OperationBuilder<AddColumnOperation> EntityValueId { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> EntityId { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> FieldId { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> ValueIndex { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> TextValue { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> IntegerValue { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> LongValue { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> DecimalValue { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> BooleanValue { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> DateValue { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> DateTimeValue { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> GuidValue { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> ReferencedEntityId { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> CreatedBy { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> CreatedOn { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> ModifiedBy { get; set; } = null!;
        public OperationBuilder<AddColumnOperation> ModifiedOn { get; set; } = null!;
    }
}
