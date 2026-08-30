using System.Collections.Generic;

namespace GIBS.Module.Entity.Helpers
{
    /// <summary>
    /// Helper methods for safely accessing query string parameters
    /// </summary>
    public static class QueryStringHelper
    {
        /// <summary>
        /// Safely gets a query string value with a default fallback
        /// </summary>
        /// <param name="queryString">The query string dictionary</param>
        /// <param name="key">The key to look up</param>
        /// <param name="defaultValue">Default value if key doesn't exist</param>
        /// <returns>The value or default</returns>
        public static string GetValueOrDefault(IDictionary<string, string> queryString, string key, string defaultValue = "")
        {
            return queryString.ContainsKey(key) ? queryString[key] : defaultValue;
        }

        /// <summary>
        /// Safely tries to get an integer value from query string
        /// </summary>
        /// <param name="queryString">The query string dictionary</param>
        /// <param name="key">The key to look up</param>
        /// <param name="value">Output parameter for the parsed value</param>
        /// <returns>True if key exists and value is valid integer</returns>
        public static bool TryGetInt(IDictionary<string, string> queryString, string key, out int value)
        {
            value = 0;
            if (!queryString.ContainsKey(key))
                return false;

            return int.TryParse(queryString[key], out value);
        }

        /// <summary>
        /// Safely gets an integer value from query string with a default fallback
        /// </summary>
        /// <param name="queryString">The query string dictionary</param>
        /// <param name="key">The key to look up</param>
        /// <param name="defaultValue">Default value if key doesn't exist or is invalid</param>
        /// <returns>The parsed integer or default</returns>
        public static int GetIntOrDefault(IDictionary<string, string> queryString, string key, int defaultValue = 0)
        {
            return TryGetInt(queryString, key, out int value) ? value : defaultValue;
        }

        /// <summary>
        /// Checks if a required query string parameter exists
        /// </summary>
        /// <param name="queryString">The query string dictionary</param>
        /// <param name="key">The key to check</param>
        /// <returns>True if key exists and is not null or empty</returns>
        public static bool HasValue(IDictionary<string, string> queryString, string key)
        {
            return queryString.ContainsKey(key) && !string.IsNullOrWhiteSpace(queryString[key]);
        }

        /// <summary>
        /// Safely tries to get a boolean value from query string
        /// </summary>
        /// <param name="queryString">The query string dictionary</param>
        /// <param name="key">The key to look up</param>
        /// <param name="value">Output parameter for the parsed value</param>
        /// <returns>True if key exists and value is valid boolean</returns>
        public static bool TryGetBool(IDictionary<string, string> queryString, string key, out bool value)
        {
            value = false;
            if (!queryString.ContainsKey(key))
                return false;

            return bool.TryParse(queryString[key], out value);
        }

        /// <summary>
        /// Safely gets a boolean value from query string with a default fallback
        /// </summary>
        /// <param name="queryString">The query string dictionary</param>
        /// <param name="key">The key to look up</param>
        /// <param name="defaultValue">Default value if key doesn't exist or is invalid</param>
        /// <returns>The parsed boolean or default</returns>
        public static bool GetBoolOrDefault(IDictionary<string, string> queryString, string key, bool defaultValue = false)
        {
            return TryGetBool(queryString, key, out bool value) ? value : defaultValue;
        }
    }
}
