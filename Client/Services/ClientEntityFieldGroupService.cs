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
    public class ClientEntityFieldGroupService : ServiceBase, IEntityFieldGroupService, IClientService
    {
        public ClientEntityFieldGroupService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("EntityFieldGroup");

        public async Task<List<EntityFieldGroup>> GetFieldGroupsAsync(int entityTypeId, int moduleId)
        {
            List<EntityFieldGroup> fieldGroups = await GetJsonAsync<List<EntityFieldGroup>>($"{ApiUrl}?entitytypeid={entityTypeId}&moduleid={moduleId}");
            return fieldGroups.OrderBy(item => item.SortOrder).ThenBy(item => item.Name).ToList();
        }

        public async Task<EntityFieldGroup> GetFieldGroupAsync(int fieldGroupId, int moduleId)
        {
            return await GetJsonAsync<EntityFieldGroup>($"{ApiUrl}/{fieldGroupId}?moduleid={moduleId}");
        }

        public async Task<EntityFieldGroup> GetFieldGroupByKeyAsync(int entityTypeId, string key, int moduleId)
        {
            return await GetJsonAsync<EntityFieldGroup>($"{ApiUrl}/key/{key}?entitytypeid={entityTypeId}&moduleid={moduleId}");
        }

        public async Task<EntityFieldGroup> AddFieldGroupAsync(EntityFieldGroup fieldGroup)
        {
            return await PostJsonAsync<EntityFieldGroup>(ApiUrl, fieldGroup);
        }

        public async Task<EntityFieldGroup> UpdateFieldGroupAsync(EntityFieldGroup fieldGroup)
        {
            return await PutJsonAsync<EntityFieldGroup>($"{ApiUrl}/{fieldGroup.FieldGroupId}", fieldGroup);
        }

        public async Task DeleteFieldGroupAsync(int fieldGroupId, int moduleId)
        {
            await DeleteAsync($"{ApiUrl}/{fieldGroupId}?moduleid={moduleId}");
        }
    }
}
