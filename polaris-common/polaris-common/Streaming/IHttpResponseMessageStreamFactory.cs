namespace polaris_common.Streaming;

public interface IHttpResponseMessageStreamFactory
{
    Task<HttpResponseMessageStream> Create(HttpResponseMessage httpResponseMessage);
}