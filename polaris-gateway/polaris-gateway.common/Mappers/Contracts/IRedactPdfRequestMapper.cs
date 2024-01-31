using Common.Dto.Request;
using Common.ValueObjects;

namespace PolarisGateway.common.Mappers.Contracts
{
    public interface IRedactPdfRequestMapper
    {
        RedactPdfRequestDto Map(DocumentRedactionSaveRequestDto saveRequest, long caseId, PolarisDocumentId polarisDocumentId, Guid correlationId);
    }
}
