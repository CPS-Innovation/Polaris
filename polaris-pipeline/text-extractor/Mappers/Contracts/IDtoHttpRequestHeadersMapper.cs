using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace text_extractor.Mappers.Contracts
{
    public interface IDtoHttpRequestHeadersMapper
    {
        T Map<T>(HttpHeaders headers);
        
        T Map<T>(IHeaderDictionary headers);
    }
}