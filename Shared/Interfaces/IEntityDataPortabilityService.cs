using System.Threading.Tasks;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Services
{
    public interface IEntityDataPortabilityService
    {
        Task<EntityDataPackage> ExportDataAsync(int siteId, int moduleId);
        Task<EntityDataImportResult> ImportDataAsync(int siteId, int moduleId, EntityDataPackage package, bool overwriteExisting = true);
        Task<byte[]> ExportDataZipAsync(int siteId, int moduleId);
        Task<EntityDataImportResult> ImportDataZipAsync(int siteId, int moduleId, byte[] zipBytes, bool overwriteExisting = true);
    }
}
