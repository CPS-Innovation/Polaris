using System;
using System.IO;
using Aspose.Email;
using Aspose.Words;
using pdf_generator.Domain.Document;
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

        public PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
        {
            var conversionResult = new PdfConversionResult(documentId, PdfConverterType.AsposeEmail);
            var mailMessageStream = new MemoryStream();
            var pdfStream = new MemoryStream();
            
            var mailMsg = _asposeItemFactory.CreateMailMessage(inputStream, correlationId);
            mailMessageStream.Seek(0, SeekOrigin.Begin);
            mailMsg.Save(mailMessageStream, SaveOptions.DefaultMhtml);

            //// load the MTHML from memoryStream into a document
            var document = _asposeItemFactory.CreateMhtmlDocument(mailMessageStream, correlationId);
            document.Save(pdfStream, SaveFormat.Pdf);
            pdfStream.Seek(0, SeekOrigin.Begin);
            
            conversionResult.RecordConversionSuccess(pdfStream);
            return conversionResult;
        }
    }
}
