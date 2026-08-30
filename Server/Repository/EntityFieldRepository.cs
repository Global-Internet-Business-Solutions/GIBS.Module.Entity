using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Repository
{
    public interface IEntityFieldRepository
    {
        IEnumerable<EntityField> GetFields(int entityTypeId);
        IEnumerable<EntityField> GetFieldsByGroup(int fieldGroupId);
        EntityField GetField(int fieldId);
        EntityField GetField(int fieldId, bool tracking);
        EntityField AddField(EntityField field);
        EntityField UpdateField(EntityField field);
        void DeleteField(int fieldId);
    }

    public class EntityFieldRepository : IEntityFieldRepository, ITransientService
    {
        private readonly IDbContextFactory<EntityContext> _factory;

        public EntityFieldRepository(IDbContextFactory<EntityContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<EntityField> GetFields(int entityTypeId)
        {
            using var db = _factory.CreateDbContext();
            return db.EntityFields
                .Where(item => item.EntityTypeId == entityTypeId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name)
                .ToList();
        }

        public IEnumerable<EntityField> GetFieldsByGroup(int fieldGroupId)
        {
            using var db = _factory.CreateDbContext();
            return db.EntityFields
                .Where(item => item.FieldGroupId == fieldGroupId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Name)
                .ToList();
        }

        public EntityField GetField(int fieldId)
        {
            return GetField(fieldId, true);
        }

        public EntityField GetField(int fieldId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.EntityFields.Find(fieldId);
            }
            else
            {
                return db.EntityFields
                    .AsNoTracking()
                    .FirstOrDefault(item => item.FieldId == fieldId);
            }
        }

        public EntityField AddField(EntityField field)
        {
            using var db = _factory.CreateDbContext();
            db.EntityFields.Add(field);
            db.SaveChanges();
            return field;
        }

        public EntityField UpdateField(EntityField field)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(field).State = EntityState.Modified;
            db.SaveChanges();
            return field;
        }

        public void DeleteField(int fieldId)
        {
            using var db = _factory.CreateDbContext();
            EntityField field = db.EntityFields.Find(fieldId);
            if (field != null)
            {
                db.EntityFields.Remove(field);
                db.SaveChanges();
            }
        }
    }
}
