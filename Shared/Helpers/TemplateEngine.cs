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
    ///   [Field:FieldKey]    - custom field value from Entity.Settings, matched by EntityField.Key
    ///   [HtmlContent:FieldKey] - raw HtmlContent from matching EntityField definition
    /// </summary>
    public static class TemplateEngine
    {
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
