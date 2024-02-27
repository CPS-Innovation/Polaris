using System;
using System.IO;
using pdf_generator.Domain.Document;

namespace pdf_generator.Services.PdfService
{
    public interface IPdfService
    {
        PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId);
    }
}
