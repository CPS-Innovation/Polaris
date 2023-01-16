using System;

namespace PolarisGateway.Extensions;

public static class StringExtensions
{
    public static string GetUntilOrEmpty(this string text, string stopAt = "/")
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        
        var charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);
        return charLocation > 0 ? text[..charLocation] : string.Empty;
    }
}
