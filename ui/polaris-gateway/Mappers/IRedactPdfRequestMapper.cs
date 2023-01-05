using System;
using RumpoleGateway.Domain.DocumentRedaction;

namespace RumpoleGateway.Mappers
{
    public interface IRedactPdfRequestMapper
    {
        RedactPdfRequest Map(DocumentRedactionSaveRequest saveRequest, int caseId, int documentId, string fileName, Guid correlationId);
    }
}
