using System.Collections.Generic;
using System.Threading.Tasks;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Services
{
    public interface IEntityFieldOptionService
    {
        Task<List<EntityFieldOption>> GetFieldOptionsAsync(int fieldId, int moduleId);
        Task<EntityFieldOption> GetFieldOptionAsync(int fieldOptionId, int moduleId);
        Task<EntityFieldOption> AddFieldOptionAsync(EntityFieldOption fieldOption);
        Task<EntityFieldOption> UpdateFieldOptionAsync(EntityFieldOption fieldOption);
        Task DeleteFieldOptionAsync(int fieldOptionId, int moduleId);
    }
}
