
using Common.Domain.SearchIndex;
using coordinator.Durable.Payloads.Domain;

namespace coordinator.Mappers
{
    public class SearchFilterDocumentMapper : ISearchFilterDocumentMapper
    {
        public SearchFilterDocument MapToSearchFilterDocument(BaseDocumentEntity baseDocumentEntity)
        {
            return new SearchFilterDocument
            {
                DocumentId = baseDocumentEntity.DocumentId,
                CmsDocumentId = baseDocumentEntity.CmsDocumentId,
                VersionId = baseDocumentEntity.VersionId,
            };
        }
    }
}