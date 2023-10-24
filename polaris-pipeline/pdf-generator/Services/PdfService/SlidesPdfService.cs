using System;
using System.IO;
using Aspose.Slides.Export;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class SlidesPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public SlidesPdfService(IAsposeItemFactory asposeItemFactory)
        {
            _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream, Guid correlationId)
        {
            using var presentation = _asposeItemFactory.CreatePresentation(inputStream, correlationId);
            presentation.Save(pdfStream, SaveFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
