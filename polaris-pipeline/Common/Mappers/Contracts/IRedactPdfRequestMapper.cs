using Common.Domain.Requests;
using System;

namespace Common.Mappers.Contracts
{
    public interface IRedactPdfRequestMapper
    {
        RedactPdfRequest Map(DocumentRedactionSaveRequest saveRequest, long caseId, Guid documentId, Guid correlationId);
    }
}
