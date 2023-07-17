using Common.Domain.Entity;
using Common.Domain.SearchIndex;
using Common.Dto.Request.Search;
using Common.Mappers.Contracts;

namespace Common.Mappers
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