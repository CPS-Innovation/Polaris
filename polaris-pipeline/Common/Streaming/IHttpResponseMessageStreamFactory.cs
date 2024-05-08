using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Streaming;

public interface IHttpResponseMessageStreamFactory
{
    Task<HttpResponseMessageStream> Create(HttpResponseMessage httpResponseMessage);
}