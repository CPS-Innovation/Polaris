using System;
using System.IO;
using System.Threading.Tasks;
using Aspose.Diagram;
using pdf_generator.Domain.Document;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class DiagramPdfService(IAsposeItemFactory asposeItemFactory) : IPdfService
    {
        public PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
        {
            var conversionResult = new PdfConversionResult(documentId, PdfConverterType.AsposeDiagrams);
            var pdfStream = new MemoryStream();

            var doc = asposeItemFactory.CreateDiagram(inputStream, correlationId);
            doc.Save(pdfStream, SaveFileFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);

            conversionResult.RecordConversionSuccess(pdfStream);
            return conversionResult;
        }

        public Task<PdfConversionResult> ReadToPdfStreamAsync(Stream inputStream, string documentId, Guid correlationId)
        {
            throw new NotImplementedException();
        }
    }
}
