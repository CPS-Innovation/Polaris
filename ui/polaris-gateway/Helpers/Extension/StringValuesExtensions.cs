using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Primitives;

namespace RumpoleGateway.Helpers.Extension
{
    [ExcludeFromCodeCoverage]
    public static class StringValuesExtensions
    {
        public static string ToJwtString(this StringValues values)
        {
            return values.ToString().Replace($"{AuthenticationKeys.Bearer} ", string.Empty).Trim();
        }
    }
}
