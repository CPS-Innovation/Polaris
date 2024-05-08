using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Streaming;

public class HttpResponseMessageStreamFactory : IHttpResponseMessageStreamFactory
{
    public Task<HttpResponseMessageStream> Create(HttpResponseMessage httpResponseMessage)
    {
        return HttpResponseMessageStream.Create(httpResponseMessage);
    }
}