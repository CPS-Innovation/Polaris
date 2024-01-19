using System;
using System.IO;
using Aspose.Email;
using Aspose.Words;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class EmailPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public EmailPdfService(IAsposeItemFactory asposeItemFactory)
        {
            _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream, Guid correlationId)
        {
            var mailMsg = _asposeItemFactory.CreateMailMessage(inputStream, correlationId);
            var memoryStream = new MemoryStream();
            memoryStream.Seek(0, SeekOrigin.Begin);
            mailMsg.Save(memoryStream, SaveOptions.DefaultMhtml);

            //// load the MTHML from memoryStream into a document
            var document = _asposeItemFactory.CreateMhtmlDocument(memoryStream, correlationId);
            document.Save(pdfStream, SaveFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
