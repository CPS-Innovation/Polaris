using System;
using System.IO;
using Common.Dto.Request;

namespace pdf_generator.Services.DocumentRedaction
{
    public interface IRedactionProvider
    {
        Stream Redact(Stream stream, string caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId);
    }
}