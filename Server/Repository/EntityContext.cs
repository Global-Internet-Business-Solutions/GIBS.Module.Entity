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

        public EntityContext(IDBContextDependencies DBContextDependencies) : base(DBContextDependencies)
        {
            // ContextBase handles multi-tenant database connections
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Models.Entity>().ToTable(ActiveDatabase.RewriteName("GIBSEntity"));
        }
    }
}
