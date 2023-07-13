using System.Net.Http.Headers;

namespace Common.Mappers.Contracts
{
    public interface IDtoHttpRequestHeadersMapper
    {
        T Map<T>(HttpHeaders headers);
    }
}