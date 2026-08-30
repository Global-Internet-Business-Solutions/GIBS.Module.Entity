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
    public class ServerEntityFieldOptionService : IEntityFieldOptionService
    {
        private readonly IEntityFieldOptionRepository _repository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerEntityFieldOptionService(
            IEntityFieldOptionRepository repository, 
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

        public Task<List<EntityFieldOption>> GetFieldOptionsAsync(int fieldId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_repository.GetFieldOptions(fieldId).ToList());
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityFieldOption GetFieldOptions Attempt {FieldId} {ModuleId}", fieldId, moduleId);
                return null;
            }
        }

        public Task<EntityFieldOption> GetFieldOptionAsync(int fieldOptionId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_repository.GetFieldOption(fieldOptionId));
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityFieldOption Get Attempt {FieldOptionId} {ModuleId}", fieldOptionId, moduleId);
                return null;
            }
        }

        public Task<EntityFieldOption> AddFieldOptionAsync(EntityFieldOption fieldOption)
        {
            if (_accessor.HttpContext.User.Identity.IsAuthenticated)
            {
                fieldOption.CreatedBy = _accessor.HttpContext.User.Identity.Name;
                fieldOption.CreatedOn = DateTime.UtcNow;
                fieldOption.ModifiedBy = fieldOption.CreatedBy;
                fieldOption.ModifiedOn = fieldOption.CreatedOn;

                fieldOption = _repository.AddFieldOption(fieldOption);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "EntityFieldOption Added {FieldOption}", fieldOption);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityFieldOption Add Attempt {FieldOption}", fieldOption);
                fieldOption = null;
            }
            return Task.FromResult(fieldOption);
        }

        public Task<EntityFieldOption> UpdateFieldOptionAsync(EntityFieldOption fieldOption)
        {
            if (_accessor.HttpContext.User.Identity.IsAuthenticated)
            {
                fieldOption.ModifiedBy = _accessor.HttpContext.User.Identity.Name;
                fieldOption.ModifiedOn = DateTime.UtcNow;

                fieldOption = _repository.UpdateFieldOption(fieldOption);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "EntityFieldOption Updated {FieldOption}", fieldOption);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityFieldOption Update Attempt {FieldOption}", fieldOption);
                fieldOption = null;
            }
            return Task.FromResult(fieldOption);
        }

        public Task DeleteFieldOptionAsync(int fieldOptionId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _repository.DeleteFieldOption(fieldOptionId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "EntityFieldOption Deleted {FieldOptionId}", fieldOptionId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityFieldOption Delete Attempt {FieldOptionId} {ModuleId}", fieldOptionId, moduleId);
            }
            return Task.CompletedTask;
        }
    }
}
