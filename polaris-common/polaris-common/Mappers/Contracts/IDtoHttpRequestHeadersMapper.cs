using System.Net.Http.Headers;

namespace polaris_common.Mappers.Contracts
{
    public interface IDtoHttpRequestHeadersMapper
    {
        T Map<T>(HttpHeaders headers);
    }
}