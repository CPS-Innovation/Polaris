using Common.Dto.Request;
using System;

namespace Common.Mappers.Contracts
{
    public interface IRedactPdfRequestMapper
    {
        RedactPdfRequestDto Map(DocumentRedactionSaveRequestDto saveRequest, long caseId, Guid documentId, Guid correlationId);
    }
}
