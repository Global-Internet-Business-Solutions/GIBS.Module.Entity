using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Repository
{
    public interface IEntityTypeRepository
    {
        IEnumerable<EntityType> GetEntityTypes(int siteId, int moduleId);
        EntityType GetEntityType(int entityTypeId);
        EntityType GetEntityType(int entityTypeId, bool tracking);
        EntityType GetEntityTypeByKey(int siteId, string key);
        EntityType AddEntityType(EntityType entityType);
        EntityType UpdateEntityType(EntityType entityType);
        void DeleteEntityType(int entityTypeId);
    }

    public class EntityTypeRepository : IEntityTypeRepository, ITransientService
    {
        private readonly IDbContextFactory<EntityContext> _factory;

        public EntityTypeRepository(IDbContextFactory<EntityContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<EntityType> GetEntityTypes(int siteId, int moduleId)
        {
            using var db = _factory.CreateDbContext();
            return db.EntityTypes
                .Where(item => item.SiteId == siteId && item.ModuleId == moduleId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name)
                .ToList();
        }

        public EntityType GetEntityType(int entityTypeId)
        {
            return GetEntityType(entityTypeId, true);
        }

        public EntityType GetEntityType(int entityTypeId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.EntityTypes.Find(entityTypeId);
            }
            else
            {
                return db.EntityTypes
                    .AsNoTracking()
                    .FirstOrDefault(item => item.EntityTypeId == entityTypeId);
            }
        }

        public EntityType GetEntityTypeByKey(int siteId, string key)
        {
            using var db = _factory.CreateDbContext();
            return db.EntityTypes
                .AsNoTracking()
                .FirstOrDefault(item => item.SiteId == siteId && item.Key == key);
        }

        public EntityType AddEntityType(EntityType entityType)
        {
            using var db = _factory.CreateDbContext();
            db.EntityTypes.Add(entityType);
            db.SaveChanges();
            return entityType;
        }

        public EntityType UpdateEntityType(EntityType entityType)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(entityType).State = EntityState.Modified;
            db.SaveChanges();
            return entityType;
        }

        public void DeleteEntityType(int entityTypeId)
        {
            using var db = _factory.CreateDbContext();
            EntityType entityType = db.EntityTypes.Find(entityTypeId);
            if (entityType != null)
            {
                db.EntityTypes.Remove(entityType);
                db.SaveChanges();
            }
        }
    }
}
