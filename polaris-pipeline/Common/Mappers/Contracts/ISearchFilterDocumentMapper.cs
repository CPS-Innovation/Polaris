using Common.Domain.SearchIndex;
using Common.Dto.Request.Search;

namespace Common.Mappers.Contracts
{
    public interface ISearchFilterDocumentMapper
    {
        SearchFilterDocument MapToSearchFilterDocument(SearchRequestDocumentDto searchRequestDocumentDto);
    }
}