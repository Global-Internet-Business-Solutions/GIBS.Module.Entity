using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using GIBS.Module.Entity.Models;
using GIBS.Module.Entity.Services;
using System.Threading.Tasks;

namespace GIBS.Module.Entity.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class EntityTypeController : Controller
    {
        private readonly IEntityTypeService _entityTypeService;
        private readonly ILogManager _logger;
        protected readonly int EntityId = -1;

        public EntityTypeController(IEntityTypeService entityTypeService, ILogManager logger, IHttpContextAccessor accessor)
        {
            _entityTypeService = entityTypeService;
            _logger = logger;
        }

        // GET: api/EntityType?siteid=x&moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> Get([FromQuery] int siteId, [FromQuery] int moduleId)
        {
            try
            {
                List<EntityType> entityTypes = await _entityTypeService.GetEntityTypesAsync(siteId, moduleId);
                return Ok(entityTypes);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Get Error {Error}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // GET api/EntityType/5?moduleid=x
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> GetEntityType(int id, [FromQuery] int moduleId)
        {
            try
            {
                EntityType entityType = await _entityTypeService.GetEntityTypeAsync(id, moduleId);
                if (entityType != null)
                {
                    return Ok(entityType);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Get Error {EntityTypeId} {Error}", id, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // GET api/EntityType/key/{key}?siteid=x&moduleid=x
        [HttpGet("key/{key}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> GetByKey(string key, [FromQuery] int siteId, [FromQuery] int moduleId)
        {
            try
            {
                EntityType entityType = await _entityTypeService.GetEntityTypeByKeyAsync(siteId, key, moduleId);
                if (entityType != null)
                {
                    return Ok(entityType);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "GetByKey Error {Key} {Error}", key, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // POST api/EntityType
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Post([FromBody] EntityType entityType)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    entityType = await _entityTypeService.AddEntityTypeAsync(entityType);
                    return Ok(entityType);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Post Error {EntityType} {Error}", entityType, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // PUT api/EntityType/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Put(int id, [FromBody] EntityType entityType)
        {
            try
            {
                if (ModelState.IsValid && entityType.EntityTypeId == id)
                {
                    entityType = await _entityTypeService.UpdateEntityTypeAsync(entityType);
                    return Ok(entityType);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Put Error {EntityType} {Error}", entityType, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // DELETE api/EntityType/5?moduleid=x
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Delete(int id, [FromQuery] int moduleId)
        {
            try
            {
                await _entityTypeService.DeleteEntityTypeAsync(id, moduleId);
                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Delete Error {EntityTypeId} {Error}", id, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
