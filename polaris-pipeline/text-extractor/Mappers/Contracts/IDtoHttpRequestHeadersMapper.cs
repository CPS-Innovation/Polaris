using System.Net.Http.Headers;

namespace text_extractor.Mappers.Contracts
{
    public interface IDtoHttpRequestHeadersMapper
    {
        T Map<T>(HttpHeaders headers);
    }
}