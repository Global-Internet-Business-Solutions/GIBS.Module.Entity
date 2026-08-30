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
    public class ServerEntityTemplateService : IEntityTemplateService
    {
        private readonly IEntityTemplateRepository _repository;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Alias _alias;

        public ServerEntityTemplateService(
            IEntityTemplateRepository repository,
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

        public Task<List<EntityTemplate>> GetTemplatesAsync(int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_repository.GetTemplates(moduleId).ToList());
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityTemplate GetTemplates Attempt {ModuleId}", moduleId);
            return null;
        }

        public Task<EntityTemplate> GetTemplateAsync(int templateId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.View))
            {
                return Task.FromResult(_repository.GetTemplate(templateId));
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityTemplate Get Attempt {TemplateId} {ModuleId}", templateId, moduleId);
            return null;
        }

        public Task<EntityTemplate> AddTemplateAsync(EntityTemplate template)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, template.ModuleId, PermissionNames.Edit))
            {
                template.CreatedBy = _accessor.HttpContext.User.Identity.Name;
                template.CreatedOn = DateTime.UtcNow;
                template.ModifiedBy = template.CreatedBy;
                template.ModifiedOn = template.CreatedOn;

                template = _repository.AddTemplate(template);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "EntityTemplate Added {Template}", template);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityTemplate Add Attempt {Template}", template);
                template = null;
            }

            return Task.FromResult(template);
        }

        public Task<EntityTemplate> UpdateTemplateAsync(EntityTemplate template)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, template.ModuleId, PermissionNames.Edit))
            {
                template.ModifiedBy = _accessor.HttpContext.User.Identity.Name;
                template.ModifiedOn = DateTime.UtcNow;

                template = _repository.UpdateTemplate(template);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "EntityTemplate Updated {Template}", template);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityTemplate Update Attempt {Template}", template);
                template = null;
            }

            return Task.FromResult(template);
        }

        public Task DeleteTemplateAsync(int templateId, int moduleId)
        {
            if (_userPermissions.IsAuthorized(_accessor.HttpContext.User, _alias.SiteId, EntityNames.Module, moduleId, PermissionNames.Edit))
            {
                _repository.DeleteTemplate(templateId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "EntityTemplate Deleted {TemplateId}", templateId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized EntityTemplate Delete Attempt {TemplateId} {ModuleId}", templateId, moduleId);
            }

            return Task.CompletedTask;
        }
    }
}
