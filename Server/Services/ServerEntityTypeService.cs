using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Security;
using Oqtane.Shared;
using GIBS.Module.Entity.Models;
using GIBS.Module.Entity.Repository;

namespace GIBS.Module.Entity.Services
{
    public class ServerEntityTypeService : IEntityTypeService
    {
        private readonly IEntityTypeRepository _entityTypeRepository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerEntityTypeService(
            IEntityTypeRepository entityTypeRepository, 
            IUserPermissions userPermissions, 
            ITenantManager tenantManager, 
            ILogManager logger, 
            IHttpContextAccessor accessor)
        {
            _entityTypeRepository = entityTypeRepository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<EntityType>> GetEntityTypesAsync(int siteId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_entityTypeRepository.GetEntityTypes(siteId, moduleId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityType GetEntityTypes Attempt {SiteId} {ModuleId}", siteId, moduleId);
                return null;
            }
        }

        public Task<EntityType> GetEntityTypeAsync(int entityTypeId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_entityTypeRepository.GetEntityType(entityTypeId));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityType Get Attempt {EntityTypeId} {ModuleId}", entityTypeId, moduleId);
                return null;
            }
        }

        public Task<EntityType> GetEntityTypeByKeyAsync(int siteId, string key, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_entityTypeRepository.GetEntityTypeByKey(siteId, key));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityType GetByKey Attempt {SiteId} {Key} {ModuleId}", siteId, key, moduleId);
                return null;
            }
        }

        public Task<EntityType> AddEntityTypeAsync(EntityType entityType)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, entityType.ModuleId, PermissionNames.Edit))
            {
                entityType.CreatedBy = _accessor.HttpContext.User.Identity.Name;
                entityType.CreatedOn = DateTime.UtcNow;
                entityType.ModifiedBy = entityType.CreatedBy;
                entityType.ModifiedOn = entityType.CreatedOn;

                entityType = _entityTypeRepository.AddEntityType(entityType);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "EntityType Added {EntityType}", entityType);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityType Add Attempt {EntityType}", entityType);
                entityType = null;
            }
            return Task.FromResult(entityType);
        }

        public Task<EntityType> UpdateEntityTypeAsync(EntityType entityType)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, entityType.ModuleId, PermissionNames.Edit))
            {
                entityType.ModifiedBy = _accessor.HttpContext.User.Identity.Name;
                entityType.ModifiedOn = DateTime.UtcNow;

                entityType = _entityTypeRepository.UpdateEntityType(entityType);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "EntityType Updated {EntityType}", entityType);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityType Update Attempt {EntityType}", entityType);
                entityType = null;
            }
            return Task.FromResult(entityType);
        }

        public Task DeleteEntityTypeAsync(int entityTypeId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _entityTypeRepository.DeleteEntityType(entityTypeId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "EntityType Deleted {EntityTypeId}", entityTypeId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityType Delete Attempt {EntityTypeId} {ModuleId}", entityTypeId, moduleId);
            }
            return Task.CompletedTask;
        }
    }
}
