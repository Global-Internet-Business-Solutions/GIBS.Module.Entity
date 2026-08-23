using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;

namespace GIBS.Module.Entity.Services
{

    public class ClientEntityService : ServiceBase, IEntityService
    {
        public ClientEntityService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Entity");

        public async Task<List<Models.Entity>> GetEntitysAsync(int ModuleId)
        {
            List<Models.Entity> Entitys = await GetJsonAsync<List<Models.Entity>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId), Enumerable.Empty<Models.Entity>().ToList());
            return Entitys.OrderBy(item => item.Name).ToList();
        }

        public async Task<Models.Entity> GetEntityAsync(int EntityId, int ModuleId)
        {
            return await GetJsonAsync<Models.Entity>(CreateAuthorizationPolicyUrl($"{Apiurl}/{EntityId}/{ModuleId}", EntityNames.Module, ModuleId));
        }

        public async Task<Models.Entity> AddEntityAsync(Models.Entity Entity)
        {
            return await PostJsonAsync<Models.Entity>(CreateAuthorizationPolicyUrl($"{Apiurl}", EntityNames.Module, Entity.ModuleId), Entity);
        }

        public async Task<Models.Entity> UpdateEntityAsync(Models.Entity Entity)
        {
            return await PutJsonAsync<Models.Entity>(CreateAuthorizationPolicyUrl($"{Apiurl}/{Entity.EntityId}", EntityNames.Module, Entity.ModuleId), Entity);
        }

        public async Task DeleteEntityAsync(int EntityId, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{EntityId}/{ModuleId}", EntityNames.Module, ModuleId));
        }
    }
}
