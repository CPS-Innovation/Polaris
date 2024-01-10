using System.Text.RegularExpressions;

namespace polaris_common.Extensions
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
    
    public static string ExtractBookendedContent(this string @this, string from = null, string until = null, StringComparison comparison = StringComparison.InvariantCulture)
    {
      var fromLength = (from ?? string.Empty).Length;
      var startIndex = !string.IsNullOrEmpty(from) 
        ? @this.IndexOf(from, comparison) + fromLength
        : 0;

      if (startIndex < fromLength) { throw new ArgumentException("from: Failed to find an instance of the first anchor"); }

      var endIndex = !string.IsNullOrEmpty(until) 
        ? @this.IndexOf(until, startIndex, comparison) 
        : @this.Length;

      if (endIndex < 0) { throw new ArgumentException("until: Failed to find an instance of the last anchor"); }

      var subString = @this.Substring(startIndex, endIndex - startIndex);
      return subString;
    }
  }
}
