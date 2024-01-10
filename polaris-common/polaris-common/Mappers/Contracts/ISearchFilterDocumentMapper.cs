using polaris_common.Domain.Entity;
using polaris_common.Domain.SearchIndex;
using polaris_common.Dto.Request.Search;

namespace polaris_common.Mappers.Contracts
{
    public interface ISearchFilterDocumentMapper
    {
        SearchFilterDocument MapToSearchFilterDocument(BaseDocumentEntity baseDocumentEntity);

        SearchFilterDocument MapToSearchFilterDocument(SearchRequestDocumentDto searchRequestDocumentDto);
    }
}