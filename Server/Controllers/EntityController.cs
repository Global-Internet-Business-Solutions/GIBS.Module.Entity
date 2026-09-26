using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using GIBS.Module.Entity.Services;
using Oqtane.Controllers;
using System.Net;
using System.Threading.Tasks;

namespace GIBS.Module.Entity.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class EntityController : ModuleControllerBase
    {
        private readonly IEntityService _EntityService;

        public EntityController(IEntityService EntityService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _EntityService = EntityService;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<IEnumerable<Models.Entity>> Get(string moduleid)
        {
            int ModuleId;
            if (int.TryParse(moduleid, out ModuleId) && IsAuthorizedEntityId(EntityNames.Module, ModuleId))
            {
                return await _EntityService.GetEntitysAsync(ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Get Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<Models.Entity> Get(int id, int moduleid)
        {
            Models.Entity Entity = await _EntityService.GetEntityAsync(id, moduleid);
            if (Entity != null && IsAuthorizedEntityId(EntityNames.Module, Entity.ModuleId))
            {
                return Entity;
            }
            else
            { 
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Get Attempt {EntityId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.Entity> Post([FromBody] Models.Entity Entity)
        {
            if (ModelState.IsValid && IsAuthorizedEntityId(EntityNames.Module, Entity.ModuleId))
            {
                Entity = await _EntityService.AddEntityAsync(Entity);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Post Attempt {Entity}", Entity);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                Entity = null;
            }
            return Entity;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<Models.Entity> Put(int id, [FromBody] Models.Entity Entity)
        {
            if (ModelState.IsValid && Entity.EntityId == id && IsAuthorizedEntityId(EntityNames.Module, Entity.ModuleId))
            {
                Entity = await _EntityService.UpdateEntityAsync(Entity);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Put Attempt {Entity}", Entity);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                Entity = null;
            }
            return Entity;
        }

        // GET api/<controller>/ensurefolder?moduleid=x&basefolderid=y&entitytypekey=a&entitykeyorname=b
        [HttpGet("ensurefolder")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<int> EnsureFolder(int moduleid, int basefolderid, string entitytypekey, string entitykeyorname)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                return await _EntityService.EnsureEntityUploadFolderAsync(moduleid, basefolderid, entitytypekey, entitykeyorname);
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity EnsureFolder Attempt {ModuleId}", moduleid);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return basefolderid;
        }

        // GET api/<controller>/movefile?moduleid=x&fileid=y&targetfolderid=z
        [HttpGet("movefile")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<int> MoveFile(int moduleid, int fileid, int targetfolderid)
        {
            if (IsAuthorizedEntityId(EntityNames.Module, moduleid))
            {
                return await _EntityService.MoveFileToFolderAsync(moduleid, fileid, targetfolderid);
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity MoveFile Attempt {ModuleId} {FileId}", moduleid, fileid);
            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return fileid;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}/{moduleid}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task Delete(int id, int moduleid)
        {
            Models.Entity Entity = await _EntityService.GetEntityAsync(id, moduleid);
            if (Entity != null && IsAuthorizedEntityId(EntityNames.Module, Entity.ModuleId))
            {
                await _EntityService.DeleteEntityAsync(id, Entity.ModuleId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Entity Delete Attempt {EntityId} {ModuleId}", id, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}
