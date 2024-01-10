using Microsoft.Extensions.Primitives;
using polaris_common.Constants;

namespace polaris_common.Extensions
{
    public static class StringValuesExtensions
    {
        public static string ToJwtString(this StringValues values)
        {
            return values.ToString().Replace($"{OAuthSettings.Bearer} ", string.Empty).Trim();
        }
    }
}
