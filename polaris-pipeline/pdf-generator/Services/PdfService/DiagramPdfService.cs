using System;
using System.IO;
using Aspose.Diagram;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class DiagramPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public DiagramPdfService(IAsposeItemFactory asposeItemFactory)
        {
            _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream, Guid correlationId)
        {
            var doc = _asposeItemFactory.CreateDiagram(inputStream, correlationId);
            doc.Save(pdfStream, SaveFileFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
