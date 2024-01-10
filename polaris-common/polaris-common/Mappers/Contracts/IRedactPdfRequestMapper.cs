using polaris_common.Dto.Request;
using polaris_common.ValueObjects;

namespace polaris_common.Mappers.Contracts
{
    public interface IRedactPdfRequestMapper
    {
        RedactPdfRequestDto Map(DocumentRedactionSaveRequestDto saveRequest, long caseId, PolarisDocumentId polarisDocumentId, Guid correlationId);
    }
}
