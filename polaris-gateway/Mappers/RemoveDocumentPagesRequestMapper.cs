using Common.Dto.Request;

namespace PolarisGateway.Mappers
{
    public class RemoveDocumentPagesRequestMapper : IRemoveDocumentPagesRequestMapper
    {
        public RemoveDocumentPagesDto Map(DocumentPageRemovalRequestDto documentPageRemovalRequest)
        {
            return new RemoveDocumentPagesDto
            {
                PagesIndexesToRemove = documentPageRemovalRequest.PagesIndexesToRemove
            };
        }
    }
}
