using Common.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace PolarisGateway.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async Task<IActionResult> ToActionResult(this HttpResponseMessage message)
    {
        if (message.Content.IsMimeMultipartContent())
        {
            return new FileStreamResult(await message.Content.ReadAsStreamAsync(), message.Content.Headers.ContentType?.MediaType);
        }

        if (message.Content.Headers.ContentType?.MediaType?.Equals(ContentType.Json) == true)
        {
            var content = await message.Content.ReadAsStringAsync();
            return new ContentResult
            {
                Content = content,
                ContentType = message.Content.Headers.ContentType?.MediaType ?? string.Empty,
                StatusCode = (int)message.StatusCode,
            };
        }

        return new StatusCodeResult((int)message.StatusCode);
    }
}