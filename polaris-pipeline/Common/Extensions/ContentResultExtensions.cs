using Microsoft.AspNetCore.Mvc;

namespace Common.Extensions;

public static class ContentResultExtensions
{
    public static bool IsSuccessStatusCode(this ContentResult contentResult)
    {
        var statusCode = contentResult.StatusCode;
        return statusCode is >= 200 and <= 299;
    }
}