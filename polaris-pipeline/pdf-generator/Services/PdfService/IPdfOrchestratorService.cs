using System;
using System.IO;
using System.Threading.Tasks;
using Common.Domain.Document;
using pdf_generator.Domain.Document;

namespace pdf_generator.Services.PdfService
{
    public interface IPdfOrchestratorService
    {
        Task<PdfConversionResult> ReadToPdfStreamAsync(Stream inputStream, FileType fileType, string documentId, Guid correlationId);
    }
}
