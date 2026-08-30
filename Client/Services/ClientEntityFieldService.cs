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
    public class ClientEntityFieldService : ServiceBase, IEntityFieldService, IClientService
    {
        public ClientEntityFieldService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("EntityField");

        public async Task<List<EntityField>> GetFieldsAsync(int entityTypeId, int moduleId)
        {
            List<EntityField> fields = await GetJsonAsync<List<EntityField>>($"{ApiUrl}?entitytypeid={entityTypeId}&moduleid={moduleId}");
            return fields.OrderBy(item => item.SortOrder).ThenBy(item => item.Name).ToList();
        }

        public async Task<List<EntityField>> GetFieldsByGroupAsync(int fieldGroupId, int moduleId)
        {
            List<EntityField> fields = await GetJsonAsync<List<EntityField>>($"{ApiUrl}/group/{fieldGroupId}?moduleid={moduleId}");
            return fields.OrderBy(item => item.SortOrder).ThenBy(item => item.Name).ToList();
        }

        public async Task<EntityField> GetFieldAsync(int fieldId, int moduleId)
        {
            return await GetJsonAsync<EntityField>($"{ApiUrl}/{fieldId}?moduleid={moduleId}");
        }

        public async Task<EntityField> AddFieldAsync(EntityField field)
        {
            return await PostJsonAsync<EntityField>(ApiUrl, field);
        }

        public async Task<EntityField> UpdateFieldAsync(EntityField field)
        {
            return await PutJsonAsync<EntityField>($"{ApiUrl}/{field.FieldId}", field);
        }

        public async Task DeleteFieldAsync(int fieldId, int moduleId)
        {
            await DeleteAsync($"{ApiUrl}/{fieldId}?moduleid={moduleId}");
        }
    }
}
