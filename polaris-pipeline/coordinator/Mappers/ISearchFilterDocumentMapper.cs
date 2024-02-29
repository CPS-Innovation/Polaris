using Common.Domain.SearchIndex;
using coordinator.Durable.Payloads.Domain;

namespace coordinator.Mappers
{
    public interface ISearchFilterDocumentMapper
    {
        SearchFilterDocument MapToSearchFilterDocument(BaseDocumentEntity baseDocumentEntity);
    }
}