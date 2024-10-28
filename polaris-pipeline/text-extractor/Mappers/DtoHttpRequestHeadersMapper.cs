using System.Net.Http.Headers;
using Common.Wrappers;
using Microsoft.AspNetCore.Http;
using text_extractor.Mappers.Contracts;

namespace text_extractor.Mappers
{
    public class DtoHttpRequestHeadersMapper : IDtoHttpRequestHeadersMapper
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public DtoHttpRequestHeadersMapper(IJsonConvertWrapper jsonConvertWrapper)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        // This is simplistic method, assumes that all DTO properties being mapped to 
        //  are castable/convertable from string.
        public T Map<T>(HttpHeaders headers)
        {
            var dict = ToDictionary(headers);
            var interimJson = _jsonConvertWrapper.SerializeObject(dict);
            return _jsonConvertWrapper.DeserializeObject<T>(interimJson);
        }

        public T Map<T>(IHeaderDictionary headers)
        {
            var dict = ToDictionary(headers);
            var interimJson = _jsonConvertWrapper.SerializeObject(dict);
            return _jsonConvertWrapper.DeserializeObject<T>(interimJson);
        }

        private static Dictionary<string, string> ToDictionary(HttpHeaders headers)
        {
            var dict = new Dictionary<string, string>();

            foreach (var item in headers.ToList())
            {
                var header = item.Value.Aggregate(string.Empty, (current, value) => current + (value + " "));

                // Trim the trailing space and add item to the dictionary
                header = header.TrimEnd(" ".ToCharArray());
                dict.Add(item.Key, header);
            }

            return dict;
        }
        
        private static Dictionary<string, string> ToDictionary(IHeaderDictionary headers)
        {
            var dict = new Dictionary<string, string>();

            foreach (var item in headers.ToList())
            {
                var header = item.Value.Aggregate(string.Empty, (current, value) => current + (value + " "));

                // Trim the trailing space and add item to the dictionary
                header = header.TrimEnd(" ".ToCharArray());
                dict.Add(item.Key, header);
            }

            return dict;
        }
    }
}