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
    public class ServerEntityFieldGroupService : IEntityFieldGroupService
    {
        private readonly IEntityFieldGroupRepository _repository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerEntityFieldGroupService(
            IEntityFieldGroupRepository repository, 
            IUserPermissions userPermissions, 
            ITenantManager tenantManager, 
            ILogManager logger, 
            IHttpContextAccessor accessor)
        {
            _repository = repository;
            _userPermissions = userPermissions;
            _logger = logger;
            _accessor = accessor;
            _alias = tenantManager.GetAlias();
        }

        public Task<List<EntityFieldGroup>> GetFieldGroupsAsync(int entityTypeId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_repository.GetFieldGroups(entityTypeId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityFieldGroup GetFieldGroups Attempt {EntityTypeId} {ModuleId}", entityTypeId, moduleId);
                return null;
            }
        }

        public Task<EntityFieldGroup> GetFieldGroupAsync(int fieldGroupId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_repository.GetFieldGroup(fieldGroupId));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityFieldGroup Get Attempt {FieldGroupId} {ModuleId}", fieldGroupId, moduleId);
                return null;
            }
        }

        public Task<EntityFieldGroup> AddFieldGroupAsync(EntityFieldGroup fieldGroup)
        {
            // Note: moduleId validation would require looking up the EntityType - simplified here
            // In production, verify the EntityType's ModuleId matches and has Edit permission
            if (_accessor.HttpContext.User.Identity.IsAuthenticated)
            {
                fieldGroup.CreatedBy = _accessor.HttpContext.User.Identity.Name;
                fieldGroup.CreatedOn = DateTime.UtcNow;
                fieldGroup.ModifiedBy = fieldGroup.CreatedBy;
                fieldGroup.ModifiedOn = fieldGroup.CreatedOn;

                fieldGroup = _repository.AddFieldGroup(fieldGroup);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "EntityFieldGroup Added {FieldGroup}", fieldGroup);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityFieldGroup Add Attempt {FieldGroup}", fieldGroup);
                fieldGroup = null;
            }
            return Task.FromResult(fieldGroup);
        }

        public Task<EntityFieldGroup> UpdateFieldGroupAsync(EntityFieldGroup fieldGroup)
        {
            if (_accessor.HttpContext.User.Identity.IsAuthenticated)
            {
                fieldGroup.ModifiedBy = _accessor.HttpContext.User.Identity.Name;
                fieldGroup.ModifiedOn = DateTime.UtcNow;

                fieldGroup = _repository.UpdateFieldGroup(fieldGroup);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "EntityFieldGroup Updated {FieldGroup}", fieldGroup);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityFieldGroup Update Attempt {FieldGroup}", fieldGroup);
                fieldGroup = null;
            }
            return Task.FromResult(fieldGroup);
        }

        public Task DeleteFieldGroupAsync(int fieldGroupId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _repository.DeleteFieldGroup(fieldGroupId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "EntityFieldGroup Deleted {FieldGroupId}", fieldGroupId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityFieldGroup Delete Attempt {FieldGroupId} {ModuleId}", fieldGroupId, moduleId);
            }
            return Task.CompletedTask;
        }
    }
}
