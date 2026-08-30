using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Services
{
    public class ClientEntityTypeService : ServiceBase, IEntityTypeService, IClientService
    {
        public ClientEntityTypeService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("EntityType");

        public async Task<List<EntityType>> GetEntityTypesAsync(int siteId, int moduleId)
        {
            List<EntityType> entityTypes = await GetJsonAsync<List<EntityType>>($"{ApiUrl}?siteid={siteId}&moduleid={moduleId}");
            return entityTypes.OrderBy(item => item.SortOrder).ThenBy(item => item.Name).ToList();
        }

        public async Task<EntityType> GetEntityTypeAsync(int entityTypeId, int moduleId)
        {
            return await GetJsonAsync<EntityType>($"{ApiUrl}/{entityTypeId}?moduleid={moduleId}");
        }

        public async Task<EntityType> GetEntityTypeByKeyAsync(int siteId, string key, int moduleId)
        {
            return await GetJsonAsync<EntityType>($"{ApiUrl}/key/{key}?siteid={siteId}&moduleid={moduleId}");
        }

        public async Task<EntityType> AddEntityTypeAsync(EntityType entityType)
        {
            return await PostJsonAsync<EntityType>(ApiUrl, entityType);
        }

        public async Task<EntityType> UpdateEntityTypeAsync(EntityType entityType)
        {
            return await PutJsonAsync<EntityType>($"{ApiUrl}/{entityType.EntityTypeId}", entityType);
        }

        public async Task DeleteEntityTypeAsync(int entityTypeId, int moduleId)
        {
            await DeleteAsync($"{ApiUrl}/{entityTypeId}?moduleid={moduleId}");
        }
    }
}
