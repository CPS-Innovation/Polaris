using System;
using System.IO;
using pdf_generator.Domain;

namespace pdf_generator.Services.PdfService
{
    public interface IPdfOrchestratorService
    {
        Stream ReadToPdfStream(Stream inputStream, FileType fileType, string documentId, Guid correlationId);
    }
}
