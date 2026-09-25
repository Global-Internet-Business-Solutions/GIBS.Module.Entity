using System.Threading.Tasks;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Services
{
    public interface IEntitySchemaPortabilityService
    {
        Task<EntitySchemaPackage> ExportSchemaAsync(int siteId, int moduleId);
        Task<EntitySchemaImportResult> ImportSchemaAsync(int siteId, int moduleId, EntitySchemaPackage package, bool overwriteExisting = true);
    }
}
