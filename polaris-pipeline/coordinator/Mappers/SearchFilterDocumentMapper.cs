using coordinator.Durable.Payloads.Domain;
using Common.Domain.SearchIndex;

namespace coordinator.Mappers
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
    }
}