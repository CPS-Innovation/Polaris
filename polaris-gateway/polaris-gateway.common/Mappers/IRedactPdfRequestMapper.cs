using System;
using PolarisGateway.Domain.DocumentRedaction;

namespace PolarisGateway.Mappers
{
    public interface IRedactPdfRequestMapper
    {
        RedactPdfRequest Map(DocumentRedactionSaveRequest saveRequest, int caseId, int documentId, string fileName, Guid correlationId);
    }
}
