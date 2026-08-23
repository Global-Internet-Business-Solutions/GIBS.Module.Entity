using Microsoft.AspNetCore.Builder; 
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using GIBS.Module.Entity.Repository;
using GIBS.Module.Entity.Services;

namespace GIBS.Module.Entity.Startup
{
    public class ServerStartup : IServerStartup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // not implemented
        }

        public void ConfigureMvc(IMvcBuilder mvcBuilder)
        {
            // not implemented
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IEntityService, ServerEntityService>();
            services.AddDbContextFactory<EntityContext>(opt => { }, ServiceLifetime.Transient);
        }
    }
}
