using System;
using System.IO;
using Common.Domain.Document;

namespace pdf_generator.Services.PdfService
{
    public interface IPdfOrchestratorService
    {
        Stream ReadToPdfStream(Stream inputStream, FileType fileType, string documentId, Guid correlationId);
    }
}
