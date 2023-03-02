using Microsoft.Extensions.Primitives;

namespace PolarisGateway.Extensions
{
    public static class StringValuesExtensions
    {
        public static string ToJwtString(this StringValues values)
        {
            return values.ToString().Replace($"{AuthenticationKeys.Bearer} ", string.Empty).Trim();
        }
    }
}
