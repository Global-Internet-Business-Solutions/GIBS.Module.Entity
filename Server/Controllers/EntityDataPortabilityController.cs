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
    public class EntityDataPortabilityController : Controller
    {
        private readonly IEntityDataPortabilityService _service;
        private readonly ILogManager _logger;

        public EntityDataPortabilityController(IEntityDataPortabilityService service, ILogManager logger)
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
                var package = await _service.ExportDataAsync(siteId, moduleId);
                return package != null ? Ok(package) : NotFound();
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "EntityData json export error {SiteId} {ModuleId}", siteId, moduleId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> ImportJson([FromQuery] int siteId, [FromQuery] int moduleId, [FromQuery] bool overwriteExisting, [FromBody] EntityDataPackage package)
        {
            try
            {
                var result = await _service.ImportDataAsync(siteId, moduleId, package, overwriteExisting);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "EntityData json import error {SiteId} {ModuleId}", siteId, moduleId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("export")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Export([FromQuery] int siteId, [FromQuery] int moduleId)
        {
            try
            {
                var zipBytes = await _service.ExportDataZipAsync(siteId, moduleId);
                var fileName = $"entity-data-{siteId}-{moduleId}.zip";
                return File(zipBytes, "application/zip", fileName);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "EntityData export error {SiteId} {ModuleId}", siteId, moduleId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("import")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<IActionResult> Import([FromQuery] int siteId, [FromQuery] int moduleId, [FromQuery] bool overwriteExisting, [FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Import ZIP file is required.");
                }

                using var stream = file.OpenReadStream();
                using var memory = new System.IO.MemoryStream();
                await stream.CopyToAsync(memory);
                var result = await _service.ImportDataZipAsync(siteId, moduleId, memory.ToArray(), overwriteExisting);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "EntityData import error {SiteId} {ModuleId}", siteId, moduleId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
