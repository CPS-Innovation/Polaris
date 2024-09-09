using Common.Dto.Request;

namespace PolarisGateway.Mappers
{
    public interface IModifyDocumentRequestMapper
    {
        ModifyDocumentDto Map(DocumentModificationRequestDto documentModificationRequest);
    }
}