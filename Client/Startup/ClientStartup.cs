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
            if (!services.Any(s => s.ServiceType == typeof(IEntityService)))
            {
                services.AddScoped<IEntityService, ClientEntityService>();
            }
        }
    }
}
