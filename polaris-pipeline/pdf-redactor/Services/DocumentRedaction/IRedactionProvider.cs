using System;
using System.IO;
using System.Threading.Tasks;
using Common.Dto.Request;

namespace pdf_redactor.Services.DocumentRedaction
{
    public interface IRedactionProvider
    {
        Task<Stream> Redact(Stream stream, string caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId);
    }
}