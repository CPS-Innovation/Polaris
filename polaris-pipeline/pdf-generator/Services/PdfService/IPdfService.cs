using System;
using System.IO;
using System.Threading.Tasks;
using pdf_generator.Domain.Document;

namespace pdf_generator.Services.PdfService
{
    public interface IPdfService
    {
        PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId);

        Task<PdfConversionResult> ReadToPdfStreamAsync(Stream inputStream, string documentId, Guid correlationId);
    }
}
