using System.Collections.Generic;
using System.Threading.Tasks;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Services
{
    public interface IEntityFieldGroupService
    {
        Task<List<EntityFieldGroup>> GetFieldGroupsAsync(int entityTypeId, int moduleId);
        Task<EntityFieldGroup> GetFieldGroupAsync(int fieldGroupId, int moduleId);
        Task<EntityFieldGroup> GetFieldGroupByKeyAsync(int entityTypeId, string key, int moduleId);
        Task<EntityFieldGroup> AddFieldGroupAsync(EntityFieldGroup fieldGroup);
        Task<EntityFieldGroup> UpdateFieldGroupAsync(EntityFieldGroup fieldGroup);
        Task DeleteFieldGroupAsync(int fieldGroupId, int moduleId);
    }
}
