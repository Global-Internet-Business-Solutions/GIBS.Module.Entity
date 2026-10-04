using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GIBS.Module.Entity.Helpers
{
    /// <summary>
    /// Key generation and validation helper for GIBS.Module.Entity.
    /// 
    /// Provides smart, conflict-aware key generation with tiered fallback strategy.
    /// Used by EntityField, EntityType, and other entities that require stable, unique keys.
    /// 
    /// Key Requirements:
    /// - Unique within their scope (e.g., per EntityType for fields)
    /// - Stable across system updates (cannot change after initial creation)
    /// - Database-safe (alphanumeric only)
    /// - Human-readable (maintains case sensitivity for readability)
    /// - URL-safe (for REST API routes)
    /// - Template-engine compatible (for [Field:KeyName] style tokens)
    /// 
    /// Usage Patterns:
    /// 1. Auto-generate on creation: User enters "Event Date", system generates "EventDate"
    /// 2. Allow manual override: User can change auto-generated key if no conflicts
    /// 3. Preserve on edit: Key remains unchanged when editing other field properties
    /// 4. Detect conflicts: Automatically add suffixes when duplicates are detected
    /// 
    /// Thread Safety: All methods are thread-safe using async/await patterns.
    /// 
    /// Versioning Notes:
    /// - v1.0.x: Original implementation - always added date suffix
    /// - v1.0.12+: Smart tiered approach - only add suffixes when conflicts detected
    /// </summary>
    public static class KeyHelper
    {
        /// <summary>
        /// Normalizes a string value into a valid key by removing non-alphanumeric characters.
        /// 
        /// Rules:
        /// - Removes all spaces, special characters, and symbols
        /// - Preserves uppercase and lowercase letters
        /// - Preserves digits (0-9)
        /// - Returns empty string if result contains no alphanumeric characters
        /// 
        /// Examples:
        /// - "Event Date & Time" → "EventDateTime"
        /// - "first-name" → "firstname"
        /// - "Email (Personal)" → "EmailPersonal"
        /// - "123-456" → "123456"
        /// - "@#$%" → "" (empty)
        /// 
        /// This normalization ensures keys are:
        /// - Database-safe (no special characters)
        /// - URL-safe (for REST APIs)
        /// - Template-engine compatible (for [Field:KeyName] tokens)
        /// </summary>
        /// <param name="value">The input string to normalize</param>
        /// <returns>Normalized key string containing only alphanumeric characters, or empty string if none exist</returns>
        public static string NormalizeKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var filtered = value.Where(char.IsLetterOrDigit).ToArray();
            return filtered.Length == 0 ? string.Empty : new string(filtered);
        }

        /// <summary>
        /// Generates a unique key from a given value using a smart tiered approach.
        /// 
        /// Key Generation Strategy (Tiered):
        /// 1. TIER 1 - Base Key: Attempts to use the normalized key as-is
        ///    - Returns immediately if available (e.g., "EventDateTime")
        ///    - Best for new fields with no conflicts
        /// 
        /// 2. TIER 2 - Date-Suffixed Key: Adds current date suffix if base key exists
        ///    - Format: "{baseKey}_{MMddyyyy}" (e.g., "EventDateTime_10042026")
        ///    - Handles cases where the same key is needed on different dates
        ///    - Returns immediately if available
        /// 
        /// 3. TIER 3 - Date + Random Suffix: Adds random number if date suffix also exists
        ///    - Format: "{baseKey}_{MMddyyyy}_{random}" (e.g., "EventDateTime_10042026_847362")
        ///    - Fallback for multiple conflicts on the same date
        ///    - Attempts up to maxAttempts times before returning with random suffix
        /// 
        /// Examples:
        /// - New field "MyNewKey" (not in database) → "MyNewKey"
        /// - Second field "MyNewKey" (first exists) → "MyNewKey_10042026"
        /// - Multiple conflicts same day → "MyNewKey_10042026_847362"
        /// 
        /// Key Normalization:
        /// - Removes all non-alphanumeric characters
        /// - Preserves letter case for display purposes
        /// - Returns empty string if no alphanumeric characters remain
        /// 
        /// Performance:
        /// - Minimal database calls when base key is available
        /// - Efficient conflict resolution with random suffix fallback
        /// - Prevents timestamp inflation for long-lived systems
        /// </summary>
        /// <param name="value">The source value to generate a key from (e.g., field name, entity name)</param>
        /// <param name="keyExistsAsync">Async delegate to check if a key already exists in the database. 
        ///                              Returns true if key exists, false if available.
        ///                              Pass null to skip uniqueness checks and return base key only.</param>
        /// <param name="maxAttempts">Maximum number of random suffix attempts before giving up (default: 25)</param>
        /// <returns>A unique key string, or empty string if value cannot be normalized</returns>
        public static async Task<string> GenerateUniqueKeyAsync(string? value, Func<string, Task<bool>> keyExistsAsync, int maxAttempts = 25)
        {
            var baseKey = NormalizeKey(value);
            if (string.IsNullOrWhiteSpace(baseKey))
            {
                return string.Empty;
            }

            if (keyExistsAsync == null)
            {
                return baseKey;
            }

            // First, check if the base key (without suffix) is available
            if (!await keyExistsAsync(baseKey))
            {
                return baseKey;
            }

            // Only add date suffix if base key already exists in database
            var datedKey = $"{baseKey}_{DateTime.Now:MMddyyyy}";

            if (!await keyExistsAsync(datedKey))
            {
                return datedKey;
            }

            // If dated key also exists, try adding random number suffixes
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var candidate = $"{datedKey}_{RandomNumberGenerator.GetInt32(100000, 1000000)}";
                if (!await keyExistsAsync(candidate))
                {
                    return candidate;
                }
            }

            return $"{datedKey}_{RandomNumberGenerator.GetInt32(100000, 1000000)}";
        }
    }
}
