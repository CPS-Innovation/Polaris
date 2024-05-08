using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Common.Domain.SearchIndex;
using Common.Dto.Request.Search;
using Common.Wrappers;

namespace coordinator.Clients.TextExtractor
{
    public class SearchDtoContentFactory : ISearchDtoContentFactory
    {
        private const string PdfContentType = "application/json";
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        public SearchDtoContentFactory(IJsonConvertWrapper jsonConvertWrapper)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        public StringContent Create(string searchTerm)
        {
            var searchDto = new SearchRequestDto
            {
                SearchTerm = searchTerm,
            };

            var json = _jsonConvertWrapper.SerializeObject(searchDto);
            return new StringContent(json, Encoding.UTF8, PdfContentType);
        }
    }
}