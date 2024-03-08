using Microsoft.Extensions.Configuration;
using System;

namespace Common.Configuration
{
    public static class IConfigurationExtension
    {
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
