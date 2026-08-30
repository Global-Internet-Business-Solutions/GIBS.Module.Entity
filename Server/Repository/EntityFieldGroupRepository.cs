using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Repository
{
    public interface IEntityFieldGroupRepository
    {
        IEnumerable<EntityFieldGroup> GetFieldGroups(int entityTypeId);
        EntityFieldGroup GetFieldGroup(int fieldGroupId);
        EntityFieldGroup GetFieldGroup(int fieldGroupId, bool tracking);
        EntityFieldGroup AddFieldGroup(EntityFieldGroup fieldGroup);
        EntityFieldGroup UpdateFieldGroup(EntityFieldGroup fieldGroup);
        void DeleteFieldGroup(int fieldGroupId);
    }

    public class EntityFieldGroupRepository : IEntityFieldGroupRepository, ITransientService
    {
        private readonly IDbContextFactory<EntityContext> _factory;

        public EntityFieldGroupRepository(IDbContextFactory<EntityContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<EntityFieldGroup> GetFieldGroups(int entityTypeId)
        {
            using var db = _factory.CreateDbContext();
            return db.EntityFieldGroups
                .Where(item => item.EntityTypeId == entityTypeId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name)
                .ToList();
        }

        public EntityFieldGroup GetFieldGroup(int fieldGroupId)
        {
            return GetFieldGroup(fieldGroupId, true);
        }

        public EntityFieldGroup GetFieldGroup(int fieldGroupId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.EntityFieldGroups.Find(fieldGroupId);
            }
            else
            {
                return db.EntityFieldGroups
                    .AsNoTracking()
                    .FirstOrDefault(item => item.FieldGroupId == fieldGroupId);
            }
        }

        public EntityFieldGroup AddFieldGroup(EntityFieldGroup fieldGroup)
        {
            using var db = _factory.CreateDbContext();
            db.EntityFieldGroups.Add(fieldGroup);
            db.SaveChanges();
            return fieldGroup;
        }

        public EntityFieldGroup UpdateFieldGroup(EntityFieldGroup fieldGroup)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(fieldGroup).State = EntityState.Modified;
            db.SaveChanges();
            return fieldGroup;
        }

        public void DeleteFieldGroup(int fieldGroupId)
        {
            using var db = _factory.CreateDbContext();
            EntityFieldGroup fieldGroup = db.EntityFieldGroups.Find(fieldGroupId);
            if (fieldGroup != null)
            {
                db.EntityFieldGroups.Remove(fieldGroup);
                db.SaveChanges();
            }
        }
    }
}
