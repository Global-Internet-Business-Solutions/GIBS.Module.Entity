using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Oqtane.Services;
using GIBS.Module.Entity.Services;

namespace GIBS.Module.Entity.Startup
{
    public class ClientStartup : IClientStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Existing Entity service
            if (!services.Any(s => s.ServiceType == typeof(IEntityService)))
            {
                services.AddScoped<IEntityService, ClientEntityService>();
            }

            // Phase 1: Entity Definition Framework services
            if (!services.Any(s => s.ServiceType == typeof(IEntityTypeService)))
            {
                services.AddScoped<IEntityTypeService, ClientEntityTypeService>();
            }
            if (!services.Any(s => s.ServiceType == typeof(IEntityTemplateService)))
            {
                services.AddScoped<IEntityTemplateService, ClientEntityTemplateService>();
            }
            if (!services.Any(s => s.ServiceType == typeof(IEntityFieldGroupService)))
            {
                services.AddScoped<IEntityFieldGroupService, ClientEntityFieldGroupService>();
            }
            if (!services.Any(s => s.ServiceType == typeof(IEntityFieldService)))
            {
                services.AddScoped<IEntityFieldService, ClientEntityFieldService>();
            }
            if (!services.Any(s => s.ServiceType == typeof(IEntityFieldOptionService)))
            {
                services.AddScoped<IEntityFieldOptionService, ClientEntityFieldOptionService>();
            }
        }
    }
}
