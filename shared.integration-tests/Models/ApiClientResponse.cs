using System.Net;
using shared.integration_tests.Extensions;

namespace shared.integration_tests.Models;

public class ApiClientResponse<T> : ApiClientResponse
{
    public ApiClientResponse(HttpResponseMessage httpResponseMessage) : base(httpResponseMessage)
    {
        ResponseObject = httpResponseMessage.IsSuccessStatusCode ? httpResponseMessage.GetContentResponseAsync<T>().Result : default;
    }
    public T? ResponseObject { get; set; }
}

public class ApiClientResponse
{
    public ApiClientResponse(HttpResponseMessage httpResponseMessage)
    {
        HttpStatusCode = httpResponseMessage.StatusCode;
        Headers = httpResponseMessage.Headers.ToDictionary();
    }
    public HttpStatusCode HttpStatusCode { get; set; }
    public IDictionary<string, IEnumerable<string>> Headers { get; set; }
}