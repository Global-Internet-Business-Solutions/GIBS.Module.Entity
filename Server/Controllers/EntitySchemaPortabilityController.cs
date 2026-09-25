using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GIBS.Module.Entity.Models;
using GIBS.Module.Entity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;

namespace GIBS.Module.Entity.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class EntitySchemaPortabilityController : Controller
    {
        private readonly IEntitySchemaPortabilityService _service;
        private readonly ILogManager _logger;

        public EntitySchemaPortabilityController(IEntitySchemaPortabilityService service, ILogManager logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Get([FromQuery] int siteId, [FromQuery] int moduleId)
        {
            try
            {
                var package = await _service.ExportSchemaAsync(siteId, moduleId);
                return package != null ? Ok(package) : NotFound();
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "EntitySchema export error {SiteId} {ModuleId}", siteId, moduleId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("export")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Export([FromQuery] int siteId, [FromQuery] int moduleId)
        {
            try
            {
                var package = await _service.ExportSchemaAsync(siteId, moduleId);
                if (package == null)
                {
                    return NotFound();
                }

                var json = JsonSerializer.Serialize(package, new JsonSerializerOptions { WriteIndented = true });
                var bytes = Encoding.UTF8.GetBytes(json);
                var fileName = $"entity-schema-{siteId}-{moduleId}.json";
                return File(bytes, "application/json", fileName);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "EntitySchema file export error {SiteId} {ModuleId}", siteId, moduleId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("import")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Import([FromQuery] int siteId, [FromQuery] int moduleId, [FromQuery] bool overwriteExisting, [FromBody] EntitySchemaPackage package)
        {
            try
            {
                var result = await _service.ImportSchemaAsync(siteId, moduleId, package, overwriteExisting);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "EntitySchema import error {SiteId} {ModuleId}", siteId, moduleId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
