using Common.Domain.SearchIndex;
using Common.Dto.Request.Search;
using text_extractor.Mappers.Contracts;

namespace Common.Mappers
{
    public class SearchFilterDocumentMapper : ISearchFilterDocumentMapper
    {
        public SearchFilterDocument MapToSearchFilterDocument(SearchRequestDocumentDto searchRequestDocumentDto)
        {
            return new SearchFilterDocument
            {
                DocumentId = searchRequestDocumentDto.DocumentId,
                VersionId = searchRequestDocumentDto.VersionId
            };
        }
    }
}