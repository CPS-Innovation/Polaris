using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common.Extensions;

public static partial class StringExtensions
{
    public static long ExtractCmsUserId(this string cookieString)
    {
        if (string.IsNullOrWhiteSpace(cookieString))
        {
            return 0;
        }

        var viaUidPattern = new Regex(@"UID=(?<uid>-?\d+)", RegexOptions.None, TimeSpan.FromMilliseconds(100));
        if (int.TryParse(viaUidPattern.Match(cookieString).Groups["uid"].Value, out var userId))
        {
            return userId;
        }

        var viaFallbackPattern = new Regex(@"CMSUSER(\d+)", RegexOptions.None, TimeSpan.FromMilliseconds(100));
        if (int.TryParse(viaFallbackPattern.Match(cookieString).Groups[1].Value, out var userIdViaFallback))
        {
            return userIdViaFallback;
        }
        // This method is used for telemetry purposes, and we may not always posses a cmsUserId
        //  in the cookie. Returning default value of 0 is enough to let us know the user id was
        //  not found.
        return 0;
    }

    public static string ExtractLoadBalancerCookies(this string cookieString)
    {
        if (string.IsNullOrWhiteSpace(cookieString))
        {
            return string.Empty;
        }

        var pattern = new Regex(@$"(BIGipServer[^;]*)", RegexOptions.None, TimeSpan.FromMilliseconds(100));
        var matches = pattern.Matches(cookieString);
        var cookies = matches.ToList().Select(m => m.Value);
        return string.Join("; ", cookies);
    }
}