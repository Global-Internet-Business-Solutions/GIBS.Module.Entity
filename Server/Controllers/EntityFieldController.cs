using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
    public class EntityFieldController : Controller
    {
        private readonly IEntityFieldService _service;
        private readonly ILogManager _logger;

        public EntityFieldController(IEntityFieldService service, ILogManager logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> Get([FromQuery] int entityTypeId, [FromQuery] int moduleId)
        {
            try
            {
                return Ok(await _service.GetFieldsAsync(entityTypeId, moduleId));
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Get Error");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("group/{fieldGroupId}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> GetByGroup(int fieldGroupId, [FromQuery] int moduleId)
        {
            try
            {
                return Ok(await _service.GetFieldsByGroupAsync(fieldGroupId, moduleId));
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "GetByGroup Error");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> GetField(int id, [FromQuery] int moduleId)
        {
            try
            {
                var item = await _service.GetFieldAsync(id, moduleId);
                return item != null ? Ok(item) : NotFound();
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Get Error {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("key/{key}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> GetByKey(string key, [FromQuery] int entityTypeId, [FromQuery] int moduleId)
        {
            try
            {
                var item = await _service.GetFieldByKeyAsync(entityTypeId, key, moduleId);
                return item != null ? Ok(item) : NotFound();
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "GetByKey Error {EntityTypeId} {Key}", entityTypeId, key);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Post([FromBody] EntityField item)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await _service.AddFieldAsync(item));
                }
                return BadRequest(ModelState);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Post Error");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Put(int id, [FromBody] EntityField item)
        {
            try
            {
                if (ModelState.IsValid && item.FieldId == id)
                {
                    return Ok(await _service.UpdateFieldAsync(item));
                }
                return BadRequest(ModelState);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Put Error");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Delete(int id, [FromQuery] int moduleId)
        {
            try
            {
                await _service.DeleteFieldAsync(id, moduleId);
                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Delete Error {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
