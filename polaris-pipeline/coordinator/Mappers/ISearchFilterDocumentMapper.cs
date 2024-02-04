using coordinator.Durable.Payloads.Domain;
using Common.Domain.SearchIndex;

namespace coordinator.Mappers
{
    public interface ISearchFilterDocumentMapper
    {
        SearchFilterDocument MapToSearchFilterDocument(BaseDocumentEntity baseDocumentEntity);

    }
}