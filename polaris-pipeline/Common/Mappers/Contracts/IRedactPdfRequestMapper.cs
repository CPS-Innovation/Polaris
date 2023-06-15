using Common.Dto.Request;
using Common.ValueObjects;
using System;

namespace Common.Mappers.Contracts
{
    public interface IRedactPdfRequestMapper
    {
        RedactPdfRequestDto Map(DocumentRedactionSaveRequestDto saveRequest, long caseId, PolarisDocumentId polarisDocumentId, Guid correlationId);
    }
}
