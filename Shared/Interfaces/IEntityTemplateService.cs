using System.Collections.Generic;
using System.Threading.Tasks;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Services
{
    public interface IEntityTemplateService
    {
        Task<List<EntityTemplate>> GetTemplatesAsync(int moduleId);
        Task<EntityTemplate> GetTemplateAsync(int templateId, int moduleId);
        Task<EntityTemplate> AddTemplateAsync(EntityTemplate template);
        Task<EntityTemplate> UpdateTemplateAsync(EntityTemplate template);
        Task DeleteTemplateAsync(int templateId, int moduleId);
    }
}
