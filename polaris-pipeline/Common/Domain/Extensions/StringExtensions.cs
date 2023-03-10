using System;

namespace Common.Domain.Extensions
{
    public static class StringExtensions
    {
        public static string UrlEncodeString(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : Uri.EscapeDataString(value);
        }

        public static string UrlDecodeString(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : Uri.UnescapeDataString(value);
        }
        public static string GetBaseUrl(this string value)
        {
            var uri = new Uri(value);
            return uri.Scheme + "://" + uri.Authority;
        }
    }
}
