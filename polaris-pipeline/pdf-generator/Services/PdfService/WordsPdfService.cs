using System;
using System.IO;
using Aspose.Words;
using Common.Domain.Document;
using Common.Extensions;
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

        public PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
        {
            var conversionResult = new PdfConversionResult(documentId, PdfConverterType.AsposeWords);
            var pdfStream = new MemoryStream();

            try
            {
                var doc = _asposeItemFactory.CreateWordsDocument(inputStream, correlationId);
                doc.Save(pdfStream, SaveFormat.Pdf);
                pdfStream.Seek(0, SeekOrigin.Begin);
            
                conversionResult.RecordConversionSuccess(pdfStream);
            }
            catch (UnsupportedFileFormatException ex)
            {
                inputStream?.Dispose();
                conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeWordsUnsupportedFileFormat,
                    ex.ToFormattedString());
            }
            catch (IncorrectPasswordException ex)
            {
                inputStream?.Dispose();
                conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeWordsPasswordProtected,
                    ex.ToFormattedString());
            }
            
            return conversionResult;
        }
    }
}
