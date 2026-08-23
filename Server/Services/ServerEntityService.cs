using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;
using GIBS.Module.Entity.Repository;

namespace GIBS.Module.Entity.Services
{
    public class ServerEntityService : IEntityService
    {
        private readonly IEntityRepository _EntityRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerEntityService(IEntityRepository EntityRepository, IUserPermissions userPermissions, ITenantManager tenantManager, ILogManager logger, IHttpContextAccessor accessor)
        {
            _EntityRepository = EntityRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<Models.Entity>> GetEntitysAsync(int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_EntityRepository.GetEntitys(ModuleId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Get Attempt {ModuleId}", ModuleId);
                return null;
            }
        }

        public Task<Models.Entity> GetEntityAsync(int EntityId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.View))
            {
                return Task.FromResult(_EntityRepository.GetEntity(EntityId));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Get Attempt {EntityId} {ModuleId}", EntityId, ModuleId);
                return null;
            }
        }

        public Task<Models.Entity> AddEntityAsync(Models.Entity Entity)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, Entity.ModuleId, PermissionNames.Edit))
            {
                Entity = _EntityRepository.AddEntity(Entity);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Entity Added {Entity}", Entity);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Add Attempt {Entity}", Entity);
                Entity = null;
            }
            return Task.FromResult(Entity);
        }

        public Task<Models.Entity> UpdateEntityAsync(Models.Entity Entity)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, Entity.ModuleId, PermissionNames.Edit))
            {
                Entity = _EntityRepository.UpdateEntity(Entity);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Entity Updated {Entity}", Entity);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Update Attempt {Entity}", Entity);
                Entity = null;
            }
            return Task.FromResult(Entity);
        }

        public Task DeleteEntityAsync(int EntityId, int ModuleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, ModuleId, PermissionNames.Edit))
            {
                _EntityRepository.DeleteEntity(EntityId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Entity Deleted {EntityId}", EntityId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Delete Attempt {EntityId} {ModuleId}", EntityId, ModuleId);
            }
            return Task.CompletedTask;
        }
    }
}
