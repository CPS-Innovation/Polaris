using Common.Dto.Request;

namespace PolarisGateway.Mappers
{
    public interface IRemoveDocumentPagesRequestMapper
    {
        RemoveDocumentPagesDto Map(DocumentPageRemovalRequestDto documentPageRemovalRequest);
    }
}