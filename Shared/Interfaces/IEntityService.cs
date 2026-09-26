using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIBS.Module.Entity.Services
{
    public interface IEntityService 
    {
        Task<List<Models.Entity>> GetEntitysAsync(int ModuleId);

        Task<Models.Entity> GetEntityAsync(int EntityId, int ModuleId);

        Task<Models.Entity> AddEntityAsync(Models.Entity Entity);

        Task<Models.Entity> UpdateEntityAsync(Models.Entity Entity);

        Task DeleteEntityAsync(int EntityId, int ModuleId);

        Task<int> EnsureEntityUploadFolderAsync(int moduleId, int baseFolderId, string entityTypeKey, string entityKeyOrName);

        Task<int> MoveFileToFolderAsync(int moduleId, int fileId, int targetFolderId);
    }
}
