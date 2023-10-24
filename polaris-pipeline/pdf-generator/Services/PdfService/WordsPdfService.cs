using System;
using System.IO;
using Aspose.Words;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class WordsPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public WordsPdfService(IAsposeItemFactory asposeItemFactory)
        {
            _asposeItemFactory = asposeItemFactory;
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream, Guid correlationId)
        {
            var doc = _asposeItemFactory.CreateWordsDocument(inputStream, correlationId);
            doc.Save(pdfStream, SaveFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
