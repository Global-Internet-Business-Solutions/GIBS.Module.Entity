using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GIBS.Module.Entity.Models;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;

namespace GIBS.Module.Entity.Services
{
    public class ClientEntityDataPortabilityService : ServiceBase, IEntityDataPortabilityService, IClientService
    {
        public ClientEntityDataPortabilityService(HttpClient http, SiteState siteState) : base(http, siteState)
        {
        }

        private string ApiUrl => CreateApiUrl("EntityDataPortability");

        public async Task<EntityDataPackage> ExportDataAsync(int siteId, int moduleId)
        {
            return await GetJsonAsync<EntityDataPackage>($"{ApiUrl}?siteid={siteId}&moduleid={moduleId}");
        }

        public async Task<EntityDataImportResult> ImportDataAsync(int siteId, int moduleId, EntityDataPackage package, bool overwriteExisting = true)
        {
            return await PostJsonAsync<EntityDataPackage, EntityDataImportResult>($"{ApiUrl}?siteid={siteId}&moduleid={moduleId}&overwriteExisting={overwriteExisting}", package);
        }

        public async Task<byte[]> ExportDataZipAsync(int siteId, int moduleId)
        {
            var response = await GetHttpClient().GetAsync($"{ApiUrl}/export?siteid={siteId}&moduleid={moduleId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<EntityDataImportResult> ImportDataZipAsync(int siteId, int moduleId, byte[] zipBytes, bool overwriteExisting = true)
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(zipBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            content.Add(fileContent, "file", "entity-data.zip");

            var response = await GetHttpClient().PostAsync($"{ApiUrl}/import?siteid={siteId}&moduleid={moduleId}&overwriteExisting={overwriteExisting}", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EntityDataImportResult>();
        }
    }
}
