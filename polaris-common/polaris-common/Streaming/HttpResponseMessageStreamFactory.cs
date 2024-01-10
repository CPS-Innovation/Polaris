namespace polaris_common.Streaming;

public class HttpResponseMessageStreamFactory : IHttpResponseMessageStreamFactory
{
    public Task<HttpResponseMessageStream> Create(HttpResponseMessage httpResponseMessage)
    {
        return HttpResponseMessageStream.Create(httpResponseMessage);
    }
}