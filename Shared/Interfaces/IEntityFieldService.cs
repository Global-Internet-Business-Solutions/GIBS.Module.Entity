using System.Collections.Generic;
using System.Threading.Tasks;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Services
{
    public interface IEntityFieldService
    {
        Task<List<EntityField>> GetFieldsAsync(int entityTypeId, int moduleId);
        Task<List<EntityField>> GetFieldsByGroupAsync(int fieldGroupId, int moduleId);
        Task<EntityField> GetFieldAsync(int fieldId, int moduleId);
        Task<EntityField> AddFieldAsync(EntityField field);
        Task<EntityField> UpdateFieldAsync(EntityField field);
        Task DeleteFieldAsync(int fieldId, int moduleId);
    }
}
