using polaris_common.Domain.Entity;
using polaris_common.Domain.SearchIndex;
using polaris_common.Dto.Request.Search;
using polaris_common.Mappers.Contracts;

namespace polaris_common.Mappers
{
    public class SearchFilterDocumentMapper : ISearchFilterDocumentMapper
    {
        public SearchFilterDocument MapToSearchFilterDocument(BaseDocumentEntity baseDocumentEntity)
        {
            return new SearchFilterDocument
            {
                CmsDocumentId = baseDocumentEntity.CmsDocumentId,
                CmsVersionId = baseDocumentEntity.CmsVersionId
            };
        }

        public SearchFilterDocument MapToSearchFilterDocument(SearchRequestDocumentDto searchRequestDocumentDto)
        {
            return new SearchFilterDocument
            {
                CmsDocumentId = searchRequestDocumentDto.CmsDocumentId,
                CmsVersionId = searchRequestDocumentDto.CmsVersionId
            };
        }
    }
}