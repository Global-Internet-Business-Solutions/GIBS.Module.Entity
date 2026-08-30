using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using GIBS.Module.Entity.Models;
using GIBS.Module.Entity.Services;

namespace GIBS.Module.Entity.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class EntityTemplateController : Controller
    {
        private readonly IEntityTemplateService _service;
        private readonly ILogManager _logger;

        public EntityTemplateController(IEntityTemplateService service, ILogManager logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> Get([FromQuery] int moduleId)
        {
            try
            {
                List<EntityTemplate> templates = await _service.GetTemplatesAsync(moduleId);
                return Ok(templates);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Get Error");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IActionResult> GetTemplate(int id, [FromQuery] int moduleId)
        {
            try
            {
                EntityTemplate template = await _service.GetTemplateAsync(id, moduleId);
                return template != null ? Ok(template) : NotFound();
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Get Error {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Post([FromBody] EntityTemplate template)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await _service.AddTemplateAsync(template));
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
        public async Task<IActionResult> Put(int id, [FromBody] EntityTemplate template)
        {
            try
            {
                if (ModelState.IsValid && template.TemplateId == id)
                {
                    return Ok(await _service.UpdateTemplateAsync(template));
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
                await _service.DeleteTemplateAsync(id, moduleId);
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
