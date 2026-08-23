using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Modules;
using Oqtane.Models;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Enums;
using Oqtane.Repository;
using GIBS.Module.Entity.Repository;
using System.Threading.Tasks;

namespace GIBS.Module.Entity.Manager
{
    public class EntityManager : MigratableModuleBase, IInstallable, IPortable, ISearchable
    {
        private readonly IEntityRepository _EntityRepository;
        private readonly IDBContextDependencies _DBContextDependencies;

        public EntityManager(IEntityRepository EntityRepository, IDBContextDependencies DBContextDependencies)
        {
            _EntityRepository = EntityRepository;
            _DBContextDependencies = DBContextDependencies;
        }

        public bool Install(Tenant tenant, string version)
        {
            return Migrate(new EntityContext(_DBContextDependencies), tenant, MigrationType.Up);
        }

        public bool Uninstall(Tenant tenant)
        {
            return Migrate(new EntityContext(_DBContextDependencies), tenant, MigrationType.Down);
        }

        public string ExportModule(Oqtane.Models.Module module)
        {
            string content = "";
            List<Models.Entity> Entitys = _EntityRepository.GetEntitys(module.ModuleId).ToList();
            if (Entitys != null)
            {
                content = JsonSerializer.Serialize(Entitys);
            }
            return content;
        }

        public void ImportModule(Oqtane.Models.Module module, string content, string version)
        {
            List<Models.Entity> Entitys = null;
            if (!string.IsNullOrEmpty(content))
            {
                Entitys = JsonSerializer.Deserialize<List<Models.Entity>>(content);
            }
            if (Entitys != null)
            {
                foreach(var Entity in Entitys)
                {
                    _EntityRepository.AddEntity(new Models.Entity { ModuleId = module.ModuleId, Name = Entity.Name });
                }
            }
        }

        public Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
           var searchContentList = new List<SearchContent>();

           foreach (var Entity in _EntityRepository.GetEntitys(pageModule.ModuleId))
           {
               if (Entity.ModifiedOn >= lastIndexedOn)
               {
                   searchContentList.Add(new SearchContent
                   {
                       EntityName = "GIBSEntity",
                       EntityId = Entity.EntityId.ToString(),
                       Title = Entity.Name,
                       Body = Entity.Name,
                       ContentModifiedBy = Entity.ModifiedBy,
                       ContentModifiedOn = Entity.ModifiedOn
                   });
               }
           }

           return Task.FromResult(searchContentList);
        }
    }
}
