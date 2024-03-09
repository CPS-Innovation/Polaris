using System;

namespace Common.Extensions
{
  public static class StringExtensions
  {
    public static string UrlDecodeString(this string value)
    {
      return string.IsNullOrWhiteSpace(value) ? string.Empty : Uri.UnescapeDataString(value);
    }

    public static string ToLowerFirstChar(this string input)
    {
      if (string.IsNullOrEmpty(input))
        return input;

      return char.ToLower(input[0]) + input.Substring(1);
    }
  }
}
