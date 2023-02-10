using Microsoft.Extensions.Configuration;
using System;

namespace Common.Configuration
{
    internal static class IConfigurationExtension
    {
        public static bool IsSettingEnabled(this IConfiguration configuration, string settingName)
        {
            string text = configuration[settingName];
            if (!string.IsNullOrEmpty(text) && (string.Compare(text, "1", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(text, "true", StringComparison.OrdinalIgnoreCase) == 0))
            {
                return true;
            }

            return false;
        }
    }
}
