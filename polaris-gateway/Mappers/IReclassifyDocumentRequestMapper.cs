using Common.Dto.Request;

namespace PolarisGateway.Mappers
{
    public interface IReclassifyDocumentRequestMapper
    {
        ReclassifyDocumentDto Map(ReclassifyDocumentDto documentReclassificationRequest);
    }
}