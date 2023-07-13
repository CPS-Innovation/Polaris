using Common.Domain.Entity;
using Common.Domain.SearchIndex;
using Common.Dto.Request.Search;

namespace Common.Mappers.Contracts
{
    public interface ISearchFilterDocumentMapper
    {
        SearchFilterDocument MapToSearchFilterDocument(BaseDocumentEntity baseDocumentEntity);

        SearchFilterDocument MapToSearchFilterDocument(SearchRequestDocumentDto searchRequestDocumentDto);
    }
}