using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using GIBS.Module.Entity.Services;
using GIBS.Module.Entity.Interfaces;
using GIBS.Module.Entity.Models;
using Oqtane.Controllers;
using System.Net;

namespace GIBS.Module.Entity.Controllers
{
    // DTO for batch entity value replacement
    public class ReplaceEntityValuesRequest
    {
        public int EntityId { get; set; }
        public List<EntityValue> Values { get; set; } = new List<EntityValue>();
        public string? ModifiedBy { get; set; }
    }

    [Route(ControllerRoutes.ApiRoute)]
    public class EntityValueController : ModuleControllerBase
    {
        private readonly IEntityValueService _entityValueService;
        private readonly IEntityService _entityService;

        public EntityValueController(IEntityValueService entityValueService, IEntityService entityService, 
            ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _entityValueService = entityValueService;
            _entityService = entityService;
        }

        // GET api/entityvalue?entityId=x&fieldId=y
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<EntityValue>> Get(int entityId, int fieldId)
        {
            try
            {
                return await _entityValueService.GetEntityValuesAsync(entityId, fieldId);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error Getting EntityValues", ex);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return null;
            }
        }

        // GET api/entityvalue/entity/5
        [HttpGet("entity/{entityId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<List<EntityValue>> GetAllByEntity(int entityId)
        {
            try
            {
                return await _entityValueService.GetAllEntityValuesAsync(entityId);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error Getting All EntityValues", ex);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return new List<EntityValue>();
            }
        }

        // GET api/entityvalue/5
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<EntityValue> Get(int id)
        {
            try
            {
                return await _entityValueService.GetEntityValueAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error Getting EntityValue", ex);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return null;
            }
        }

        // POST api/entityvalue
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<EntityValue> Post([FromBody] EntityValue entityValue)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    return await _entityValueService.AddEntityValueAsync(
                        entityValue.EntityId,
                        entityValue.FieldId,
                        entityValue.ValueIndex,
                        ExtractTypedValue(entityValue),
                        entityValue.CreatedBy);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error Creating EntityValue", ex);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Invalid EntityValue");
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }

        // PUT api/entityvalue/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<EntityValue> Put(int id, [FromBody] EntityValue entityValue)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _entityValueService.UpdateEntityValueAsync(id, ExtractTypedValue(entityValue), entityValue.ModifiedBy);
                    return await _entityValueService.GetEntityValueAsync(id);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error Updating EntityValue", ex);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Invalid EntityValue");
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }

        // PUT api/entityvalue/entity/5
        [HttpPut("entity/{entityId}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Put(int entityId, [FromBody] ReplaceEntityValuesRequest request)
        {
            try
            {
                if (request == null)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Invalid ReplaceEntityValuesRequest");
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                var values = request.Values ?? new List<EntityValue>();
                await _entityValueService.ReplaceAllEntityValuesAsync(entityId, values, request.ModifiedBy);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error Replacing EntityValues", ex);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        // DELETE api/entityvalue/5
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id)
        {
            try
            {
                await _entityValueService.DeleteEntityValueAsync(id);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error Deleting EntityValue", ex);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        // DELETE api/entityvalue/field/5/3
        [HttpDelete("field/{entityId}/{fieldId}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task DeleteField(int entityId, int fieldId)
        {
            try
            {
                await _entityValueService.DeleteFieldValuesAsync(entityId, fieldId);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error Deleting Field Values", ex);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        // DELETE api/entityvalue/entity/5
        [HttpDelete("entity/{entityId}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task DeleteEntity(int entityId)
        {
            try
            {
                await _entityValueService.DeleteEntitiesValuesAsync(new List<int> { entityId });
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Error Deleting Entity Values", ex);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        // Helper: Extract the typed value from EntityValue
        private object ExtractTypedValue(EntityValue entityValue)
        {
            if (entityValue.TextValue != null) return entityValue.TextValue;
            if (entityValue.IntegerValue.HasValue) return entityValue.IntegerValue.Value;
            if (entityValue.LongValue.HasValue) return entityValue.LongValue.Value;
            if (entityValue.DecimalValue.HasValue) return entityValue.DecimalValue.Value;
            if (entityValue.BooleanValue.HasValue) return entityValue.BooleanValue.Value;
            if (entityValue.DateValue.HasValue) return entityValue.DateValue.Value;
            if (entityValue.DateTimeValue.HasValue) return entityValue.DateTimeValue.Value;
            if (entityValue.GuidValue.HasValue) return entityValue.GuidValue.Value;
            if (entityValue.ReferencedEntityId.HasValue) return entityValue.ReferencedEntityId.Value;

            return null;
        }
    }
}
