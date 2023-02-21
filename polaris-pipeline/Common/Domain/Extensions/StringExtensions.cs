using System;
using Common.Constants;

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
    }
}
