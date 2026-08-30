using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Modules;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Repository
{
    public interface IEntityTemplateRepository
    {
        IEnumerable<EntityTemplate> GetTemplates(int moduleId);
        EntityTemplate GetTemplate(int templateId);
        EntityTemplate AddTemplate(EntityTemplate template);
        EntityTemplate UpdateTemplate(EntityTemplate template);
        void DeleteTemplate(int templateId);
    }

    public class EntityTemplateRepository : IEntityTemplateRepository, ITransientService
    {
        private readonly IDbContextFactory<EntityContext> _factory;

        public EntityTemplateRepository(IDbContextFactory<EntityContext> factory)
        {
            _factory = factory;
        }

        public IEnumerable<EntityTemplate> GetTemplates(int moduleId)
        {
            using var db = _factory.CreateDbContext();
            return db.EntityTemplates
                .Where(item => item.ModuleId == moduleId)
                .OrderBy(item => item.TemplateId)
                .ToList();
        }

        public EntityTemplate GetTemplate(int templateId)
        {
            using var db = _factory.CreateDbContext();
            return db.EntityTemplates.Find(templateId);
        }

        public EntityTemplate AddTemplate(EntityTemplate template)
        {
            using var db = _factory.CreateDbContext();
            db.EntityTemplates.Add(template);
            db.SaveChanges();
            return template;
        }

        public EntityTemplate UpdateTemplate(EntityTemplate template)
        {
            using var db = _factory.CreateDbContext();
            db.Entry(template).State = EntityState.Modified;
            db.SaveChanges();
            return template;
        }

        public void DeleteTemplate(int templateId)
        {
            using var db = _factory.CreateDbContext();
            var template = db.EntityTemplates.Find(templateId);
            if (template != null)
            {
                db.EntityTemplates.Remove(template);
                db.SaveChanges();
            }
        }
    }
}
