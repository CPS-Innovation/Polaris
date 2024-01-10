using System;
using System.IO;
using polaris_common.Dto.Request;

namespace pdf_generator.Services.DocumentRedaction
{
    public interface IRedactionProvider
    {
        Stream Redact(Stream stream, RedactPdfRequestDto redactPdfRequest, Guid correlationId);
    }
}