using System.Net.Http.Headers;
using polaris_common.Mappers.Contracts;
using polaris_common.Wrappers.Contracts;

namespace polaris_common.Mappers
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