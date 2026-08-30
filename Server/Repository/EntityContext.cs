using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Infrastructure;
using Oqtane.Repository.Databases.Interfaces;

namespace GIBS.Module.Entity.Repository
{
    public class EntityContext : DBContextBase, ITransientService, IMultiDatabase
    {
        public virtual DbSet<Models.Entity> Entity { get; set; }
        public virtual DbSet<Models.EntityType> EntityTypes { get; set; }
        public virtual DbSet<Models.EntityTemplate> EntityTemplates { get; set; }
        public virtual DbSet<Models.EntityFieldGroup> EntityFieldGroups { get; set; }
        public virtual DbSet<Models.EntityField> EntityFields { get; set; }
        public virtual DbSet<Models.EntityFieldOption> EntityFieldOptions { get; set; }

        public EntityContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies)
        {
            // ContextBase handles multi-tenant database connections
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Models.Entity>().ToTable(ActiveDatabase.RewriteName("GIBS_Entity"));
            builder.Entity<Models.EntityType>().ToTable(ActiveDatabase.RewriteName("GIBS_EntityType"));
            builder.Entity<Models.EntityTemplate>().ToTable(ActiveDatabase.RewriteName("GIBS_EntityTemplate"));
            builder.Entity<Models.EntityFieldGroup>().ToTable(ActiveDatabase.RewriteName("GIBS_EntityFieldGroup"));
            builder.Entity<Models.EntityField>().ToTable(ActiveDatabase.RewriteName("GIBS_EntityField"));
            builder.Entity<Models.EntityFieldOption>().ToTable(ActiveDatabase.RewriteName("GIBS_EntityFieldOption"));
        }
    }
}
