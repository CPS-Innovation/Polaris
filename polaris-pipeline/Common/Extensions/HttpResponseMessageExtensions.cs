using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Common.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async Task<ContentResult> ToContentResultAsync(this HttpResponseMessage responseMessage)
    {
        var contentResult = new ContentResult
        {
            StatusCode = (int)responseMessage.StatusCode,
            Content = await responseMessage.Content.ReadAsStringAsync(),
            ContentType = responseMessage.Content.Headers.ContentType?.MediaType
        };
        return contentResult;
    }

    public static async Task<FileStreamResult> ToFileStreamResultAsync(this HttpResponseMessage responseMessage)
    {
        var fileStreamResult = new FileStreamResult(await responseMessage.Content.ReadAsStreamAsync(), responseMessage.Content.Headers.ContentType?.MediaType);
        return fileStreamResult;
    }
}