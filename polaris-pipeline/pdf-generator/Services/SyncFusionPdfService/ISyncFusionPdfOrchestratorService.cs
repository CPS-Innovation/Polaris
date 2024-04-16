using System;
using System.IO;
using Common.Domain.Document;
using pdf_generator.Domain.Document;

namespace pdf_generator.Services.PdfService
{
    public interface ISyncFusionPdfOrchestratorService
    {
        SyncFusionPdfConversionResult ReadToPdfStream(Stream inputStream, FileType fileType, string documentId, Guid correlationId);
    }
}
