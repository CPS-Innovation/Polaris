using Common.Domain.SearchIndex;
using Common.Dto.Request.Search;

namespace text_extractor.Mappers.Contracts
{
    public interface ISearchFilterDocumentMapper
    {
        SearchFilterDocument MapToSearchFilterDocument(SearchRequestDocumentDto searchRequestDocumentDto);
    }
}