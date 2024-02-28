using Common.Domain.SearchIndex;
using coordinator.Domain.Entity;

namespace coordinator.Mappers
{
    public interface ISearchFilterDocumentMapper
    {
        SearchFilterDocument MapToSearchFilterDocument(BaseDocumentEntity baseDocumentEntity);
    }
}