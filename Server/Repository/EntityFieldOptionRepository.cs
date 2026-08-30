using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Repository
{
    public interface IEntityFieldOptionRepository
    {
        IEnumerable<EntityFieldOption> GetFieldOptions(int fieldId);
        EntityFieldOption GetFieldOption(int fieldOptionId);
        EntityFieldOption GetFieldOption(int fieldOptionId, bool tracking);
        EntityFieldOption AddFieldOption(EntityFieldOption fieldOption);
        EntityFieldOption UpdateFieldOption(EntityFieldOption fieldOption);
        void DeleteFieldOption(int fieldOptionId);
    }

    public class EntityFieldOptionRepository : IEntityFieldOptionRepository, ITransientService
    {
        private readonly IDbContextFactory<EntityContext> _factory;

        public EntityFieldOptionRepository(IDbContextFactory<EntityContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<EntityFieldOption> GetFieldOptions(int fieldId)
        {
            using var db = _factory.CreateDbContext();
            return db.EntityFieldOptions
                .Where(item => item.FieldId == fieldId)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.DisplayText)
                .ToList();
        }

        public EntityFieldOption GetFieldOption(int fieldOptionId)
        {
            return GetFieldOption(fieldOptionId, true);
        }

        public EntityFieldOption GetFieldOption(int fieldOptionId, bool tracking)
        {
            using var db = _factory.CreateDbContext();
            if (tracking)
            {
                return db.EntityFieldOptions.Find(fieldOptionId);
            }
            else
            {
                return db.EntityFieldOptions
                    .AsNoTracking()
                    .FirstOrDefault(item => item.FieldOptionId == fieldOptionId);
            }
        }

        public EntityFieldOption AddFieldOption(EntityFieldOption fieldOption)
        {
            using var db = _factory.CreateDbContext();
            db.EntityFieldOptions.Add(fieldOption);
            db.SaveChanges();
            return fieldOption;
        }

        public EntityFieldOption UpdateFieldOption(EntityFieldOption fieldOption)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(fieldOption).State = EntityState.Modified;
            db.SaveChanges();
            return fieldOption;
        }

        public void DeleteFieldOption(int fieldOptionId)
        {
            using var db = _factory.CreateDbContext();
            EntityFieldOption fieldOption = db.EntityFieldOptions.Find(fieldOptionId);
            if (fieldOption != null)
            {
                db.EntityFieldOptions.Remove(fieldOption);
                db.SaveChanges();
            }
        }
    }
}
