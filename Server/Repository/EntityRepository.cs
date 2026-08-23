using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;

namespace GIBS.Module.Entity.Repository
{
    public interface IEntityRepository
    {
        IEnumerable<Models.Entity> GetEntitys(int ModuleId);
        Models.Entity GetEntity(int EntityId);
        Models.Entity GetEntity(int EntityId, bool tracking);
        Models.Entity AddEntity(Models.Entity Entity);
        Models.Entity UpdateEntity(Models.Entity Entity);
        void DeleteEntity(int EntityId);
    }

    public class EntityRepository : IEntityRepository, ITransientService
    {
        private readonly IDbContextFactory<EntityContext> _factory;

        public EntityRepository(IDbContextFactory<EntityContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<Models.Entity> GetEntitys(int ModuleId)
        {
            using var db = _factory.CreateDbContext();
            return db.Entity.Where(item => item.ModuleId == ModuleId).ToList();
        }

        public Models.Entity GetEntity(int EntityId)
        {
            return GetEntity(EntityId, true);
        }

        public Models.Entity GetEntity(int EntityId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.Entity.Find(EntityId);
            }
            else
            {
                return db.Entity.AsNoTracking().FirstOrDefault(item => item.EntityId == EntityId);
            }
        }

        public Models.Entity AddEntity(Models.Entity Entity)
        {
            using var db = _factory.CreateDbContext();
            db.Entity.Add(Entity);
            db.SaveChanges();
            return Entity;
        }

        public Models.Entity UpdateEntity(Models.Entity Entity)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(Entity).State = EntityState.Modified;
            db.SaveChanges();
            return Entity;
        }

        public void DeleteEntity(int EntityId)
        {
            using var db = _factory.CreateDbContext();
            Models.Entity Entity = db.Entity.Find(EntityId);
            db.Entity.Remove(Entity);
            db.SaveChanges();
        }
    }
}
