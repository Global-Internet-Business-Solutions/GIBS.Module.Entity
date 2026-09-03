using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Helpers
{
    /// <summary>
    /// Resolves token-based HTML templates for EntityType list, detail, search, and featured views.
    /// Supported tokens:
    ///   [Name]              - Entity.Name
    ///   [Key]               - Entity.Key
    ///   [Status]            - Entity.Status
    ///   [SortOrder]         - Entity.SortOrder
    ///   [IsEnabled]         - Entity.IsEnabled
    ///   [Field:FieldKey]    - custom field value from Entity.Settings (legacy) or EntityValue table
    ///   [HtmlContent:FieldKey] - raw HtmlContent from matching EntityField definition
    /// </summary>
    public static class TemplateEngine
    {
        /// <summary>
        /// Render template with legacy Settings-based custom field values
        /// </summary>
        public static string Render(
            string template,
            Models.Entity entity,
            IEnumerable<EntityField> fields)
        {
            if (string.IsNullOrWhiteSpace(template) || entity == null)
            {
                return string.Empty;
            }

            var customValues = ParseCustomFieldValues(entity.Settings);
            var fieldsByKey = BuildFieldsByKey(fields);
            var result = new StringBuilder(template);

            // Standard tokens
            result.Replace("[Name]", entity.Name ?? string.Empty);
            result.Replace("[Key]", entity.Key ?? string.Empty);
            result.Replace("[Status]", entity.Status ?? string.Empty);
            result.Replace("[SortOrder]", entity.SortOrder.ToString());
            result.Replace("[IsEnabled]", entity.IsEnabled.ToString());

            // Custom field tokens
            foreach (var field in fieldsByKey.Values)
            {
                var fieldToken = $"[Field:{field.Key}]";
                var htmlToken = $"[HtmlContent:{field.Key}]";

                if (template.Contains(fieldToken))
                {
                    customValues.TryGetValue(field.FieldId, out var fieldValue);
                    result.Replace(fieldToken, fieldValue ?? string.Empty);
                }

                if (template.Contains(htmlToken))
                {
                    result.Replace(htmlToken, field.HtmlContent ?? string.Empty);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Render template with EntityValue-based custom field values (normalized table)
        /// </summary>
        public static string Render(
            string template,
            Models.Entity entity,
            IEnumerable<EntityField> fields,
            IEnumerable<EntityValue> entityValues)
        {
            if (string.IsNullOrWhiteSpace(template) || entity == null)
            {
                return string.Empty;
            }

            // Build custom values map from EntityValue records
            var customValues = BuildCustomValuesFromEntityValues(entityValues);
            var fieldsByKey = BuildFieldsByKey(fields);
            var result = new StringBuilder(template);

            // Standard tokens
            result.Replace("[Name]", entity.Name ?? string.Empty);
            result.Replace("[Key]", entity.Key ?? string.Empty);
            result.Replace("[Status]", entity.Status ?? string.Empty);
            result.Replace("[SortOrder]", entity.SortOrder.ToString());
            result.Replace("[IsEnabled]", entity.IsEnabled.ToString());

            // Custom field tokens
            foreach (var field in fieldsByKey.Values)
            {
                var fieldToken = $"[Field:{field.Key}]";
                var htmlToken = $"[HtmlContent:{field.Key}]";

                if (template.Contains(fieldToken))
                {
                    customValues.TryGetValue(field.FieldId, out var fieldValue);
                    result.Replace(fieldToken, fieldValue ?? string.Empty);
                }

                if (template.Contains(htmlToken))
                {
                    result.Replace(htmlToken, field.HtmlContent ?? string.Empty);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Build custom field values dictionary from EntityValue records
        /// Handles multiple values by concatenating with comma separator
        /// </summary>
        private static Dictionary<int, string> BuildCustomValuesFromEntityValues(IEnumerable<EntityValue> entityValues)
        {
            var result = new Dictionary<int, string>();

            if (entityValues == null)
            {
                return result;
            }

            // Group by FieldId and concatenate values
            var groupedByField = new Dictionary<int, List<string>>();

            foreach (var ev in entityValues)
            {
                if (!groupedByField.ContainsKey(ev.FieldId))
                {
                    groupedByField[ev.FieldId] = new List<string>();
                }

                // Extract the actual value based on data type
                var stringValue = ExtractStringValue(ev);
                if (!string.IsNullOrEmpty(stringValue))
                {
                    groupedByField[ev.FieldId].Add(stringValue);
                }
            }

            // Join multiple values with comma separator
            foreach (var kvp in groupedByField)
            {
                result[kvp.Key] = string.Join(", ", kvp.Value);
            }

            return result;
        }

        /// <summary>
        /// Extract the string representation of an EntityValue based on its data type
        /// </summary>
        private static string ExtractStringValue(EntityValue ev)
        {
            if (ev.TextValue != null)
                return ev.TextValue;
            if (ev.IntegerValue.HasValue)
                return ev.IntegerValue.ToString();
            if (ev.LongValue.HasValue)
                return ev.LongValue.ToString();
            if (ev.DecimalValue.HasValue)
                return ev.DecimalValue.ToString();
            if (ev.BooleanValue.HasValue)
                return ev.BooleanValue.ToString();
            if (ev.DateValue.HasValue)
                return ev.DateValue.Value.ToString("yyyy-MM-dd");
            if (ev.DateTimeValue.HasValue)
                return ev.DateTimeValue.Value.ToString("yyyy-MM-dd HH:mm:ss");
            if (ev.GuidValue.HasValue)
                return ev.GuidValue.ToString();

            return string.Empty;
        }

        private static Dictionary<int, string> ParseCustomFieldValues(string settings)
        {
            if (string.IsNullOrWhiteSpace(settings))
            {
                return new Dictionary<int, string>();
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<int, string>>(settings)
                    ?? new Dictionary<int, string>();
            }
            catch
            {
                return new Dictionary<int, string>();
            }
        }

        private static Dictionary<string, EntityField> BuildFieldsByKey(IEnumerable<EntityField> fields)
        {
            var result = new Dictionary<string, EntityField>(System.StringComparer.OrdinalIgnoreCase);
            if (fields == null)
            {
                return result;
            }

            foreach (var field in fields)
            {
                if (!string.IsNullOrWhiteSpace(field.Key))
                {
                    result.TryAdd(field.Key, field);
                }
            }

            return result;
        }
    }
}
