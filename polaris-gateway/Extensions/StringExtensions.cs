using System.Text.RegularExpressions;

namespace PolarisGateway.Extensions
{
  public static class StringExtensions
  {
    public static long ExtractCmsUserId(this string cookieString)
    {
      if (string.IsNullOrWhiteSpace(cookieString))
      {
        return 0;
      }

      var pattern = new Regex(@$"UID=(\d+)", RegexOptions.None, TimeSpan.FromMilliseconds(100));
      var match = pattern.Match(cookieString);
      if (!long.TryParse(match.Groups[1].Value, out var cmsUserId))
      {
        // This method is used for telemetry purposes, and we may not always posses a cmsUserId
        //  in the cookie. Returning default value of 0 is enough to let us know the user id was
        //  not found.
        return 0;
      };
      return cmsUserId;
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
}
