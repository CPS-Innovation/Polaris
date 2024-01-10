using Microsoft.Extensions.Configuration;

namespace polaris_common.Configuration
{
    public static class ConfigurationExtension
    {
        public static bool IsConfigSettingEnabled(this IConfiguration configuration, string settingName)
        {
            string text = configuration[settingName];
            if (!string.IsNullOrEmpty(text) && (string.Compare(text, "1", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(text, "true", StringComparison.OrdinalIgnoreCase) == 0))
            {
                return true;
            }

            return false;
        }

        public static string GetValueFromConfig(this IConfiguration configuration, string secretName)
        {
            var secret = configuration[secretName];
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new Exception($"Secret cannot be null: {secretName}");
            }

            return secret;
        }

    }
}
