using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Services
{
    public class ClientEntitySchemaPortabilityService : ServiceBase, IEntitySchemaPortabilityService, IClientService
    {
        public ClientEntitySchemaPortabilityService(HttpClient http, SiteState siteState) : base(http, siteState)
        {
        }

        private string ApiUrl => CreateApiUrl("EntitySchemaPortability");

        public async Task<EntitySchemaPackage> ExportSchemaAsync(int siteId, int moduleId)
        {
            return await GetJsonAsync<EntitySchemaPackage>($"{ApiUrl}?siteid={siteId}&moduleid={moduleId}");
        }

        public async Task<EntitySchemaImportResult> ImportSchemaAsync(int siteId, int moduleId, EntitySchemaPackage package, bool overwriteExisting = true)
        {
            return await PostJsonAsync<EntitySchemaPackage, EntitySchemaImportResult>($"{ApiUrl}/import?siteid={siteId}&moduleid={moduleId}&overwriteExisting={overwriteExisting}", package);
        }
    }
}
