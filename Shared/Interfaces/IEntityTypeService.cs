using System.Collections.Generic;
using System.Threading.Tasks;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Services
{
    public interface IEntityTypeService
    {
        Task<List<EntityType>> GetEntityTypesAsync(int siteId, int moduleId);
        Task<EntityType> GetEntityTypeAsync(int entityTypeId, int moduleId);
        Task<EntityType> GetEntityTypeByKeyAsync(int siteId, string key, int moduleId);
        Task<EntityType> AddEntityTypeAsync(EntityType entityType);
        Task<EntityType> UpdateEntityTypeAsync(EntityType entityType);
        Task DeleteEntityTypeAsync(int entityTypeId, int moduleId);
    }
}
