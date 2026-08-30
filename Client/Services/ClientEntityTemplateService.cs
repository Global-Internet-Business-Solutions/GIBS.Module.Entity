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
    public class ClientEntityTemplateService : ServiceBase, IEntityTemplateService, IClientService
    {
        public ClientEntityTemplateService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("EntityTemplate");

        public async Task<List<EntityTemplate>> GetTemplatesAsync(int moduleId)
        {
            List<EntityTemplate> templates = await GetJsonAsync<List<EntityTemplate>>($"{ApiUrl}?moduleid={moduleId}");
            return templates.OrderBy(item => item.TemplateId).ToList();
        }

        public async Task<EntityTemplate> GetTemplateAsync(int templateId, int moduleId)
        {
            return await GetJsonAsync<EntityTemplate>($"{ApiUrl}/{templateId}?moduleid={moduleId}");
        }

        public async Task<EntityTemplate> AddTemplateAsync(EntityTemplate template)
        {
            return await PostJsonAsync<EntityTemplate>(ApiUrl, template);
        }

        public async Task<EntityTemplate> UpdateTemplateAsync(EntityTemplate template)
        {
            return await PutJsonAsync<EntityTemplate>($"{ApiUrl}/{template.TemplateId}", template);
        }

        public async Task DeleteTemplateAsync(int templateId, int moduleId)
        {
            await DeleteAsync($"{ApiUrl}/{templateId}?moduleid={moduleId}");
        }
    }
}
