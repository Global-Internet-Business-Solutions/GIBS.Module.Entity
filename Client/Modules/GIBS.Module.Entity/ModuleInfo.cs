using Oqtane.Models;
using Oqtane.Modules;

namespace GIBS.Module.Entity
{
    public class ModuleInfo : IModule
    {
        public ModuleDefinition ModuleDefinition => new ModuleDefinition
        {
            Name = "Entity",
            Description = "User defined entity definitions, grouped fields, typed values, searchable, filterable metadata, and entity specific templates.",
            Version = "1.0.0",
            ServerManagerType = "GIBS.Module.Entity.Manager.EntityManager, GIBS.Module.Entity.Server.Oqtane",
            ReleaseVersions = "1.0.0",
            Dependencies = "GIBS.Module.Entity.Shared.Oqtane",
            PackageName = "GIBS.Module.Entity" 
        };
    }
}
