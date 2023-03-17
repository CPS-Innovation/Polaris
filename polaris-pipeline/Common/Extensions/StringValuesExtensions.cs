using Common.Constants;
using Microsoft.Extensions.Primitives;

namespace Common.Extensions
{
    public static class StringValuesExtensions
    {
        public static string ToJwtString(this StringValues values)
        {
            return values.ToString().Replace($"{OAuthSettings.Bearer} ", string.Empty).Trim();
        }
    }
}
