using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using GIBS.Module.Entity.Enums;
using GIBS.Module.Entity.Models;

namespace GIBS.Module.Entity.Helpers
{
    /// <summary>
    /// Resolves token-based HTML templates for EntityType list, detail, search, and featured views.
    /// Supported tokens:
    ///   [Name]              - Entity.Name
    ///   [Key]               - Entity.Key
    ///   [Status]            - Entity.Status
    ///   [Type]              - Entity.EntityType.Name (or EntityTypeId fallback)
    ///   [DateCreated]       - Entity.CreatedOn (default format)
    ///   [DateCreated:format] - Entity.CreatedOn with custom DateTime format
    ///   [DateModified]      - Entity.ModifiedOn (default format)
    ///   [DateModified:format] - Entity.ModifiedOn with custom DateTime format
    ///   [SortOrder]         - Entity.SortOrder
    ///   [IsEnabled]         - Entity.IsEnabled
    ///   [Field:FieldKey]    - custom field value from Entity.Settings (legacy) or EntityValue table (comma-separated for multi-value)
    ///   [Field:FieldKey:Index] - indexed custom field value for multi-value fields (for example image list)
    ///   [FieldList:FieldKey] - unordered list (<ul><li>...</li></ul>) for multi-value fields
    ///   [FieldGroup:FieldGroupKey] - two-column table (Label | Value) for fields in the specified group
    ///   [HtmlContent:FieldKey] - raw HTML from the record value, or field-level HtmlContent when no record value exists
    ///   [ViewLink]...[/ViewLink] - hyperlink to current page with ?detail=Entity.Key and class="viewLink"
    ///   [Edit]              - edit hyperlink with pencil icon for current entity
    ///   [HASIMAGES]...[/HASIMAGES] - shows block only when the record has ImageUpload values
    ///   [HASNOIMAGES]...[/HASNOIMAGES] - shows block only when the record has no ImageUpload values
    /// </summary>
    public static class TemplateEngine
    {
        /// <summary>
        /// Render template with legacy Settings-based custom field values
        /// </summary>
        public static string Render(
            string template,
            Models.Entity entity,
            IEnumerable<EntityField> fields,
            string viewLinkBaseUrl = null,
            string editLinkBaseUrl = null,
            IEnumerable<EntityFieldGroup> fieldGroups = null)
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
            result.Replace("[Type]", GetEntityTypeTokenValue(entity));
            ApplyDateToken(result, "DateCreated", entity.CreatedOn);
            ApplyDateToken(result, "DateModified", entity.ModifiedOn);
            result.Replace("[SortOrder]", entity.SortOrder.ToString());
            result.Replace("[IsEnabled]", entity.IsEnabled.ToString());

            var hasImages = fieldsByKey.Values.Any(field =>
                field.EditorType == EntityEditorType.ImageUpload &&
                customValues.TryGetValue(field.FieldId, out var value) &&
                GetIndexedValues(value).Any(v => !string.IsNullOrWhiteSpace(v)));
            ApplyImageConditionalBlocks(result, hasImages);

            // Custom field tokens
            foreach (var field in fieldsByKey.Values)
            {
                var fieldToken = $"[Field:{field.Key}]";
                var fieldListToken = $"[FieldList:{field.Key}]";
                var htmlToken = $"[HtmlContent:{field.Key}]";

                customValues.TryGetValue(field.FieldId, out var fieldValue);

                if (template.Contains(fieldToken))
                {
                    result.Replace(fieldToken, fieldValue ?? string.Empty);
                }

                var indexedValues = GetIndexedValues(fieldValue);
                if (template.Contains(fieldListToken))
                {
                    result.Replace(fieldListToken, BuildUnorderedList(indexedValues));
                }

                // Supports indexed token, for example: [Field:photo:0]
                ApplyIndexedFieldTokens(result, field.Key, indexedValues);

                if (template.Contains(htmlToken))
                {
                    result.Replace(htmlToken, ResolveHtmlTokenValue(field, fieldValue));
                }
            }

            ApplyFieldGroupToken(result, fieldsByKey.Values, fieldGroups, fieldId =>
            {
                customValues.TryGetValue(fieldId, out var groupFieldValue);
                return GetIndexedValues(groupFieldValue);
            });
            ApplyViewLinkToken(result, entity, viewLinkBaseUrl);
            ApplyEditToken(result, entity, editLinkBaseUrl);
            return result.ToString();
        }

        /// <summary>
        /// Render template with EntityValue-based custom field values (normalized table)
        /// </summary>
        public static string Render(
            string template,
            Models.Entity entity,
            IEnumerable<EntityField> fields,
            IEnumerable<EntityValue> entityValues,
            string viewLinkBaseUrl = null,
            string editLinkBaseUrl = null,
            IEnumerable<EntityFieldGroup> fieldGroups = null)
        {
            if (string.IsNullOrWhiteSpace(template) || entity == null)
            {
                return string.Empty;
            }

            // Build custom values map from EntityValue records
            var customValues = BuildCustomValuesFromEntityValues(entityValues);
            var customValueLists = BuildCustomValueListsFromEntityValues(entityValues);
            var fieldsByKey = BuildFieldsByKey(fields);
            var result = new StringBuilder(template);

            // Standard tokens
            result.Replace("[Name]", entity.Name ?? string.Empty);
            result.Replace("[Key]", entity.Key ?? string.Empty);
            result.Replace("[Status]", entity.Status ?? string.Empty);
            result.Replace("[Type]", GetEntityTypeTokenValue(entity));
            ApplyDateToken(result, "DateCreated", entity.CreatedOn);
            ApplyDateToken(result, "DateModified", entity.ModifiedOn);
            result.Replace("[SortOrder]", entity.SortOrder.ToString());
            result.Replace("[IsEnabled]", entity.IsEnabled.ToString());

            var hasImages = fieldsByKey.Values.Any(field =>
                field.EditorType == EntityEditorType.ImageUpload &&
                customValueLists.TryGetValue(field.FieldId, out var values) &&
                values.Any(v => !string.IsNullOrWhiteSpace(v)));
            ApplyImageConditionalBlocks(result, hasImages);

            // Custom field tokens
            foreach (var field in fieldsByKey.Values)
            {
                var fieldToken = $"[Field:{field.Key}]";
                var fieldListToken = $"[FieldList:{field.Key}]";
                var htmlToken = $"[HtmlContent:{field.Key}]";

                customValues.TryGetValue(field.FieldId, out var fieldValue);

                if (template.Contains(fieldToken))
                {
                    result.Replace(fieldToken, fieldValue ?? string.Empty);
                }

                var indexedValues = customValueLists.TryGetValue(field.FieldId, out var values)
                    ? values.SelectMany(v => GetIndexedValues(v)).Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    : new List<string>();

                if (template.Contains(fieldListToken))
                {
                    result.Replace(fieldListToken, BuildUnorderedList(indexedValues));
                }

                // Supports indexed token, for example: [Field:photo:0]
                ApplyIndexedFieldTokens(result, field.Key, indexedValues);

                if (template.Contains(htmlToken))
                {
                    result.Replace(htmlToken, ResolveHtmlTokenValue(field, fieldValue));
                }
            }

            ApplyFieldGroupToken(result, fieldsByKey.Values, fieldGroups, fieldId =>
                customValueLists.TryGetValue(fieldId, out var groupValues)
                    ? groupValues.SelectMany(v => GetIndexedValues(v)).Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    : new List<string>());
            ApplyViewLinkToken(result, entity, viewLinkBaseUrl);
            ApplyEditToken(result, entity, editLinkBaseUrl);
            return result.ToString();
        }

        private static string BuildUnorderedList(IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return string.Empty;
            }

            var listItems = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => $"<li>{WebUtility.HtmlEncode(v)}</li>")
                .ToList();

            if (listItems.Count == 0)
            {
                return string.Empty;
            }

            return $"<ul>{string.Join(string.Empty, listItems)}</ul>";
        }

        private static void ApplyFieldGroupToken(StringBuilder result, IEnumerable<EntityField> fields, IEnumerable<EntityFieldGroup> fieldGroups, Func<int, IReadOnlyList<string>> getFieldValues)
        {
            if (result.Length == 0)
            {
                return;
            }

            var content = result.ToString();
            var pattern = @"\[FieldGroup:([^\]]+)\]";
            if (!Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
            {
                return;
            }

            var groupLookup = (fieldGroups ?? new List<EntityFieldGroup>())
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .GroupBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            var fieldList = (fields ?? new List<EntityField>()).ToList();

            content = Regex.Replace(content, pattern, match =>
            {
                var groupKey = match.Groups[1].Value?.Trim();
                if (string.IsNullOrWhiteSpace(groupKey) || !groupLookup.TryGetValue(groupKey, out var fieldGroup))
                {
                    return string.Empty;
                }

                var groupedFields = fieldList
                    .Where(f => f.FieldGroupId == fieldGroup.FieldGroupId)
                    .OrderBy(f => f.SortOrder)
                    .ThenBy(f => f.Name)
                    .ToList();

                if (!groupedFields.Any())
                {
                    return string.Empty;
                }

                var rows = groupedFields.Select(field =>
                {
                    var label = string.IsNullOrWhiteSpace(field.Label) ? field.Name : field.Label;
                    var values = (getFieldValues?.Invoke(field.FieldId) ?? new List<string>())
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .ToList();
                    var value = string.Join(", ", values);
                    return $"<tr><td class=\"text-end\"><span class=\"fw-bold\">{WebUtility.HtmlEncode(label)}:</span></td><td>{WebUtility.HtmlEncode(value)}</td></tr>";
                }).ToList();

                if (!rows.Any())
                {
                    return string.Empty;
                }

                return $"<table class=\"table table-striped table-sm\"><tbody>{string.Join(string.Empty, rows)}</tbody></table>";
            }, RegexOptions.IgnoreCase);

            result.Clear();
            result.Append(content);
        }

        private static void ApplyViewLinkToken(StringBuilder result, Models.Entity entity, string viewLinkBaseUrl)
        {
            if (result.Length == 0 || entity == null)
            {
                return;
            }

            var content = result.ToString();
            if (!Regex.IsMatch(content, @"\:?\[ViewLink\]|\[/ViewLink\]", RegexOptions.IgnoreCase))
            {
                return;
            }

            var detailKey = !string.IsNullOrWhiteSpace(entity.Key)
                ? Uri.EscapeDataString(entity.Key)
                : entity.EntityId.ToString();

            var baseUrl = string.IsNullOrWhiteSpace(viewLinkBaseUrl) ? string.Empty : viewLinkBaseUrl;
            var separator = baseUrl.Contains("?", StringComparison.Ordinal) ? "&" : "?";
            var href = string.IsNullOrWhiteSpace(baseUrl)
                ? $"?detail={detailKey}"
                : $"{baseUrl}{separator}detail={detailKey}";
            var openTag = $"<a href=\"{href}\" class=\"viewLink\">";

            content = Regex.Replace(content, @"\:?\[ViewLink\]", openTag, RegexOptions.IgnoreCase);
            content = Regex.Replace(content, @"\[/ViewLink\]", "</a>", RegexOptions.IgnoreCase);

            result.Clear();
            result.Append(content);
        }

        private static void ApplyEditToken(StringBuilder result, Models.Entity entity, string editLinkBaseUrl)
        {
            if (result.Length == 0 || entity == null)
            {
                return;
            }

            var content = result.ToString();
            if (!Regex.IsMatch(content, @"\[Edit\]", RegexOptions.IgnoreCase))
            {
                return;
            }

            var baseUrl = string.IsNullOrWhiteSpace(editLinkBaseUrl) ? string.Empty : editLinkBaseUrl;
            var separator = baseUrl.Contains("?", StringComparison.Ordinal) ? "&" : "?";
            var href = string.IsNullOrWhiteSpace(baseUrl)
                ? $"?id={entity.EntityId}"
                : $"{baseUrl}{separator}id={entity.EntityId}";
            var editMarkup = $"<a href=\"{href}\"><i class=\"oi oi-pencil\"></i></a> ";

            content = Regex.Replace(content, @"\[Edit\]", editMarkup, RegexOptions.IgnoreCase);

            result.Clear();
            result.Append(content);
        }

        private static string ResolveHtmlTokenValue(EntityField field, string fieldValue)
        {
            if (!string.IsNullOrWhiteSpace(fieldValue))
            {
                return fieldValue;
            }

            return field?.HtmlContent ?? string.Empty;
        }

        /// <summary>
        /// Build custom field values dictionary from EntityValue records
        /// Handles multiple values by concatenating with comma separator
        /// </summary>
        private static Dictionary<int, string> BuildCustomValuesFromEntityValues(IEnumerable<EntityValue> entityValues)
        {
            var result = new Dictionary<int, string>();
            var groupedByField = BuildCustomValueListsFromEntityValues(entityValues);

            foreach (var kvp in groupedByField)
            {
                result[kvp.Key] = string.Join(", ", kvp.Value);
            }

            return result;
        }

        private static Dictionary<int, List<string>> BuildCustomValueListsFromEntityValues(IEnumerable<EntityValue> entityValues)
        {
            var result = new Dictionary<int, List<string>>();

            if (entityValues == null)
            {
                return result;
            }

            foreach (var ev in entityValues.OrderBy(e => e.FieldId).ThenBy(e => e.ValueIndex))
            {
                if (!result.ContainsKey(ev.FieldId))
                {
                    result[ev.FieldId] = new List<string>();
                }

                if (!string.IsNullOrWhiteSpace(ev.TextValue))
                {
                    if (TryExtractImagePaths(ev.TextValue, out var imagePaths) && imagePaths.Count > 0)
                    {
                        result[ev.FieldId].AddRange(imagePaths.Where(p => !string.IsNullOrWhiteSpace(p)));
                    }
                    else
                    {
                        result[ev.FieldId].Add(ev.TextValue);
                    }
                }
                else
                {
                    var stringValue = ExtractStringValue(ev);
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        result[ev.FieldId].Add(stringValue);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Extract the string representation of an EntityValue based on its data type
        /// </summary>
        private static string ExtractStringValue(EntityValue ev)
        {
            if (ev.TextValue != null)
            {
                if (TryExtractImagePaths(ev.TextValue, out var imagePaths) && imagePaths.Count > 0)
                {
                    return imagePaths[0];
                }
                return ev.TextValue;
            }
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

        private static List<string> GetIndexedValues(string? fieldValue)
        {
            if (string.IsNullOrWhiteSpace(fieldValue))
            {
                return new List<string>();
            }

            if (TryExtractImagePaths(fieldValue, out var imagePaths) && imagePaths.Count > 0)
            {
                return imagePaths;
            }

            return fieldValue.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .ToList();
        }

        private static bool TryExtractImagePaths(string textValue, out List<string> filePaths)
        {
            filePaths = new List<string>();

            if (string.IsNullOrWhiteSpace(textValue))
            {
                return false;
            }

            var trimmed = textValue.Trim();
            if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
            {
                return false;
            }

            // Handle legacy concatenated object format: {..},{..}
            if (trimmed.StartsWith("{") && trimmed.Contains("},{", System.StringComparison.Ordinal))
            {
                trimmed = $"[{trimmed.Replace("}, {", "},{")}]";
            }

            try
            {
                using var doc = JsonDocument.Parse(trimmed);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (TryGetFilePath(root, out var path))
                    {
                        filePaths.Add(path);
                    }
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in root.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Object && TryGetFilePath(item, out var path))
                        {
                            filePaths.Add(path);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return filePaths.Count > 0;
        }

        private static bool TryGetFilePath(JsonElement element, out string filePath)
        {
            filePath = string.Empty;

            if (element.TryGetProperty("filePath", out var pathProp) && pathProp.ValueKind == JsonValueKind.String)
            {
                var path = pathProp.GetString();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    filePath = path;
                    return true;
                }
            }

            return false;
        }

        private static void ApplyIndexedFieldTokens(StringBuilder result, string fieldKey, IReadOnlyList<string> values)
        {
            if (string.IsNullOrWhiteSpace(fieldKey) || result.Length == 0)
            {
                return;
            }

            var pattern = $@"\[Field:{Regex.Escape(fieldKey)}:(\d+)\]";
            var content = result.ToString();

            if (!Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
            {
                return;
            }

            var replaced = Regex.Replace(content, pattern, match =>
            {
                if (!int.TryParse(match.Groups[1].Value, out var index) || index < 0)
                {
                    return string.Empty;
                }

                return index < values.Count ? values[index] ?? string.Empty : string.Empty;
            }, RegexOptions.IgnoreCase);

            result.Clear();
            result.Append(replaced);
        }

        private static void ApplyImageConditionalBlocks(StringBuilder result, bool hasImages)
        {
            if (result.Length == 0)
            {
                return;
            }

            var content = result.ToString();

            content = Regex.Replace(content, @"\[HASIMAGES\](.*?)\[/HASIMAGES\]", match =>
            {
                return hasImages ? match.Groups[1].Value : string.Empty;
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            content = Regex.Replace(content, @"\[HASNOIMAGES\](.*?)\[/HASNOIMAGES\]", match =>
            {
                return hasImages ? string.Empty : match.Groups[1].Value;
            }, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Always remove any stray markers so tokens never render literally
            content = Regex.Replace(content, @"\[/?HASIMAGES\]", string.Empty, RegexOptions.IgnoreCase);
            content = Regex.Replace(content, @"\[/?HASNOIMAGES\]", string.Empty, RegexOptions.IgnoreCase);

            result.Clear();
            result.Append(content);
        }

        private static string GetEntityTypeTokenValue(Models.Entity entity)
        {
            if (!string.IsNullOrWhiteSpace(entity.EntityType?.Name))
            {
                return entity.EntityType.Name;
            }

            if (!string.IsNullOrWhiteSpace(entity.EntityType?.Key))
            {
                return entity.EntityType.Key;
            }

            return entity.EntityTypeId > 0 ? entity.EntityTypeId.ToString() : string.Empty;
        }

        private static void ApplyDateToken(StringBuilder result, string tokenName, System.DateTime value)
        {
            if (result.Length == 0 || string.IsNullOrWhiteSpace(tokenName))
            {
                return;
            }

            var content = result.ToString();
            var pattern = $@"\[{Regex.Escape(tokenName)}(?::([^\]]+))?\]";

            if (!Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
            {
                return;
            }

            var replaced = Regex.Replace(content, pattern, match =>
            {
                if (value == default)
                {
                    return string.Empty;
                }

                var format = match.Groups[1].Success ? match.Groups[1].Value : "yyyy-MM-dd HH:mm:ss";
                try
                {
                    return value.ToString(format);
                }
                catch
                {
                    return value.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }, RegexOptions.IgnoreCase);

            result.Clear();
            result.Append(replaced);
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
