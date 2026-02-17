using System.Net.Http;

namespace Common.LayerResponse;

public class HttpError : Error
{
    public HttpError(HttpResponseMessage httpResponseMessage)
    {
        HttpStatusCode = httpResponseMessage.StatusCode;
        Message = httpResponseMessage.ReasonPhrase;
    }
}
