using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Services;
using Oqtane.Shared;
using GIBS.Module.Entity.Models;
using GIBS.Module.Entity.Interfaces;

namespace GIBS.Module.Entity.Services
{
    public class ClientEntityValueService : ServiceBase, IEntityValueService
    {
        public ClientEntityValueService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("EntityValue");

        // CRUD Operations

        public async Task<EntityValue> AddEntityValueAsync(int entityId, int fieldId, int valueIndex, 
            object typedValue, string? createdBy = null)
        {
            var entityValue = new EntityValue
            {
                EntityId = entityId,
                FieldId = fieldId,
                ValueIndex = valueIndex,
                CreatedBy = createdBy
            };

            return await PostJsonAsync<EntityValue>(
                CreateAuthorizationPolicyUrl($"{Apiurl}", EntityNames.Module, 0), 
                entityValue);
        }

        public async Task AddEntityValuesAsync(int entityId, int fieldId, List<object> typedValues, 
            string? createdBy = null)
        {
            for (int i = 0; i < typedValues.Count; i++)
            {
                await AddEntityValueAsync(entityId, fieldId, i, typedValues[i], createdBy);
            }
        }

        public async Task<List<EntityValue>> GetEntityValuesAsync(int entityId, int fieldId)
        {
            return await GetJsonAsync<List<EntityValue>>(
                CreateAuthorizationPolicyUrl($"{Apiurl}?entityId={entityId}&fieldId={fieldId}", EntityNames.Module, 0),
                Enumerable.Empty<EntityValue>().ToList());
        }

        public async Task<EntityValue?> GetEntityValueAsync(int entityValueId)
        {
            return await GetJsonAsync<EntityValue>(
                CreateAuthorizationPolicyUrl($"{Apiurl}/{entityValueId}", EntityNames.Module, 0));
        }

        public async Task UpdateEntityValueAsync(int entityValueId, object typedValue, 
            string? modifiedBy = null)
        {
            var entityValue = await GetEntityValueAsync(entityValueId);
            if (entityValue != null)
            {
                entityValue.ModifiedBy = modifiedBy;
                await PutJsonAsync(
                    CreateAuthorizationPolicyUrl($"{Apiurl}/{entityValueId}", EntityNames.Module, 0), 
                    entityValue);
            }
        }

        public async Task ReplaceEntityFieldValuesAsync(int entityId, int fieldId, List<object> typedValues,
            string? modifiedBy = null)
        {
            await DeleteAsync(
                CreateAuthorizationPolicyUrl($"{Apiurl}/field/{entityId}/{fieldId}", EntityNames.Module, 0));

            await AddEntityValuesAsync(entityId, fieldId, typedValues, modifiedBy);
        }

        public async Task DeleteEntityValueAsync(int entityValueId)
        {
            await DeleteAsync(
                CreateAuthorizationPolicyUrl($"{Apiurl}/{entityValueId}", EntityNames.Module, 0));
        }

        public async Task DeleteFieldValuesAsync(int entityId, int fieldId)
        {
            await DeleteAsync(
                CreateAuthorizationPolicyUrl($"{Apiurl}/field/{entityId}/{fieldId}", EntityNames.Module, 0));
        }

        // Search & Filter

        public async Task<List<Models.Entity>> SearchEntityValuesAsync(List<EntityFieldFilter> filters,
            int? sortFieldId = null, bool sortDescending = false,
            int skip = 0, int take = 20)
        {
            // This would require a more complex API endpoint
            // For now, return empty list
            return new List<Models.Entity>();
        }

        public async Task<int> CountEntityValuesAsync(List<EntityFieldFilter> filters)
        {
            // This would require a more complex API endpoint
            // For now, return 0
            return 0;
        }

        // Bulk operations

        public async Task DeleteEntitiesValuesAsync(List<int> entityIds)
        {
            foreach (var entityId in entityIds)
            {
                await DeleteAsync(
                    CreateAuthorizationPolicyUrl($"{Apiurl}/entity/{entityId}", EntityNames.Module, 0));
            }
        }

        /// <summary>
        /// Gets all entity values for a specific entity across all fields.
        /// </summary>
        public async Task<List<EntityValue>> GetAllEntityValuesAsync(int entityId)
        {
            return await GetJsonAsync<List<EntityValue>>(
                CreateAuthorizationPolicyUrl($"{Apiurl}/entity/{entityId}", EntityNames.Module, 0),
                Enumerable.Empty<EntityValue>().ToList());
        }

        /// <summary>
        /// Replaces all entity values for a specific entity. Deletes existing values and inserts new ones.
        /// </summary>
        public async Task ReplaceAllEntityValuesAsync(int entityId, List<EntityValue> values, string? modifiedBy = null)
        {
            var request = new 
            { 
                EntityId = entityId, 
                Values = values, 
                ModifiedBy = modifiedBy 
            };

            await PutJsonAsync<object>(
                CreateAuthorizationPolicyUrl($"{Apiurl}/entity/{entityId}", EntityNames.Module, 0), 
                request);
        }
    }
}
