using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Common.Mappers.Contracts;
using Common.Wrappers;

namespace Common.Mappers
{
    public class DtoHttpRequestHeadersMapper : IDtoHttpRequestHeadersMapper
    {
        private readonly JsonConvertWrapper _jsonConvertWrapper;

        public DtoHttpRequestHeadersMapper(JsonConvertWrapper jsonConvertWrapper)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public T Map<T>(HttpHeaders headers)
        {
            var dict = ToDictionary(headers);
            var interimJson = _jsonConvertWrapper.SerializeObject(dict);
            return _jsonConvertWrapper.DeserializeObject<T>(interimJson);
        }

        private Dictionary<string, string> ToDictionary(HttpHeaders headers)
        {
            var dict = new Dictionary<string, string>();

            foreach (var item in headers.ToList())
            {
                if (item.Value != null)
                {
                    var header = String.Empty;
                    foreach (var value in item.Value)
                    {
                        header += value + " ";
                    }

                    // Trim the trailing space and add item to the dictionary
                    header = header.TrimEnd(" ".ToCharArray());
                    dict.Add(item.Key, header);
                }
            }

            return dict;
        }
    }
}