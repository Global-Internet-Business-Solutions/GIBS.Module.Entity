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
    public class ClientEntityFieldOptionService : ServiceBase, IEntityFieldOptionService, IClientService
    {
        public ClientEntityFieldOptionService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("EntityFieldOption");

        public async Task<List<EntityFieldOption>> GetFieldOptionsAsync(int fieldId, int moduleId)
        {
            List<EntityFieldOption> fieldOptions = await GetJsonAsync<List<EntityFieldOption>>($"{ApiUrl}?fieldid={fieldId}&moduleid={moduleId}");
            return fieldOptions.OrderBy(item => item.SortOrder).ThenBy(item => item.DisplayText).ToList();
        }

        public async Task<EntityFieldOption> GetFieldOptionAsync(int fieldOptionId, int moduleId)
        {
            return await GetJsonAsync<EntityFieldOption>($"{ApiUrl}/{fieldOptionId}?moduleid={moduleId}");
        }

        public async Task<EntityFieldOption> AddFieldOptionAsync(EntityFieldOption fieldOption)
        {
            return await PostJsonAsync<EntityFieldOption>(ApiUrl, fieldOption);
        }

        public async Task<EntityFieldOption> UpdateFieldOptionAsync(EntityFieldOption fieldOption)
        {
            return await PutJsonAsync<EntityFieldOption>($"{ApiUrl}/{fieldOption.FieldOptionId}", fieldOption);
        }

        public async Task DeleteFieldOptionAsync(int fieldOptionId, int moduleId)
        {
            await DeleteAsync($"{ApiUrl}/{fieldOptionId}?moduleid={moduleId}");
        }
    }
}
