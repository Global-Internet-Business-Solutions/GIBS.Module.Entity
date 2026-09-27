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
    public class ServerEntityFieldService : IEntityFieldService
    {
        private readonly IEntityFieldRepository _repository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerEntityFieldService(
            IEntityFieldRepository repository, 
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

        public Task<List<EntityField>> GetFieldsAsync(int entityTypeId, int moduleId)
        {
            var user = _accessor.HttpContext?.User;
            if (user != null && _userPermissions.IsAuthorized(user, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_repository.GetFields(entityTypeId).ToList());
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityField GetFields Attempt {EntityTypeId} {ModuleId}", entityTypeId, moduleId);
            return Task.FromResult(new List<EntityField>());
        }

        public Task<List<EntityField>> GetFieldsByGroupAsync(int fieldGroupId, int moduleId)
        {
            var user = _accessor.HttpContext?.User;
            if (user != null && _userPermissions.IsAuthorized(user, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_repository.GetFieldsByGroup(fieldGroupId).ToList());
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityField GetFieldsByGroup Attempt {FieldGroupId} {ModuleId}", fieldGroupId, moduleId);
            return Task.FromResult(new List<EntityField>());
        }

        public Task<EntityField> GetFieldAsync(int fieldId, int moduleId)
        {
            var user = _accessor.HttpContext?.User;
            if (user != null && _userPermissions.IsAuthorized(user, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_repository.GetField(fieldId));
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityField Get Attempt {FieldId} {ModuleId}", fieldId, moduleId);
            return Task.FromResult<EntityField>(null);
        }

        public Task<EntityField> GetFieldByKeyAsync(int entityTypeId, string key, int moduleId)
        {
            var user = _accessor.HttpContext?.User;
            if (user != null && _userPermissions.IsAuthorized(user, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_repository.GetFieldByKey(entityTypeId, key));
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityField GetByKey Attempt {EntityTypeId} {Key} {ModuleId}", entityTypeId, key, moduleId);
            return Task.FromResult<EntityField>(null);
        }

        public Task<EntityField> AddFieldAsync(EntityField field)
        {
            var user = _accessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                field.CreatedBy = user.Identity.Name;
                field.CreatedOn = DateTime.UtcNow;
                field.ModifiedBy = field.CreatedBy;
                field.ModifiedOn = field.CreatedOn;

                field = _repository.AddField(field);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "EntityField Added {Field}", field);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityField Add Attempt {Field}", field);
                field = null;
            }
            return Task.FromResult(field);
        }

        public Task<EntityField> UpdateFieldAsync(EntityField field)
        {
            var user = _accessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                field.ModifiedBy = user.Identity.Name;
                field.ModifiedOn = DateTime.UtcNow;

                field = _repository.UpdateField(field);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "EntityField Updated {Field}", field);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityField Update Attempt {Field}", field);
                field = null;
            }
            return Task.FromResult(field);
        }

        public Task DeleteFieldAsync(int fieldId, int moduleId)
        {
            var user = _accessor.HttpContext?.User;
            if (user != null && _userPermissions.IsAuthorized(user, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _repository.DeleteField(fieldId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "EntityField Deleted {FieldId}", fieldId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityField Delete Attempt {FieldId} {ModuleId}", fieldId, moduleId);
            }
            return Task.CompletedTask;
        }
    }
}
