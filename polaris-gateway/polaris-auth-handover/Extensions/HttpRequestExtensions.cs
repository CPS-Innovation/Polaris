using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace PolarisAuthHandover.Extensions
{
    public static class HttpRequestExtensions
    {
        private const string XForwardedForHeaderName = "X-Forwarded-For";

        private const string EmptyClientIpAddress = "0.0.0.0";

        private const string CmsAuthCookieName = ".CMSAUTH";

        private const string CmsAuthCookieContentReplacementText = "REDACTED";

        public static string GetClientIpAddress(this HttpRequest req)
        {
            return req.Headers[XForwardedForHeaderName]
                .FirstOrDefault()
                ?.Split(new char[] { ',' })
                .FirstOrDefault()
                ?.Split(new char[] { ':' })
                .FirstOrDefault()
                ?? EmptyClientIpAddress;
        }

        public static string GetLogSafeQueryString(this HttpRequest req)
        {
            var queryString = req.QueryString.ToString();
            // we are trying not log the full .CMSAUTH cookie so we're not logging auth info
            return Regex.Replace(
                queryString,
                $"({CmsAuthCookieName})(=|%3D)(.*?)(;|%3B|$)",
                $"$1$2{CmsAuthCookieContentReplacementText}$4",
                RegexOptions.None,
                TimeSpan.FromSeconds(1));
        }
    }
}
