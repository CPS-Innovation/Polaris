using System;
using System.IO;

namespace pdf_generator.Services.PdfService
{
    public interface IPdfService
    {
        void ReadToPdfStream(Stream inputStream, Stream pdfStream, Guid correlationId);
    }
}
