using System;
using System.IO;
using System.Threading.Tasks;
using Aspose.Words;
using Common.Constants;
using pdf_generator.Domain.Document;
using pdf_generator.Extensions;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class HtePdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public HtePdfService(IAsposeItemFactory asposeItemFactory)
        {
            _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
        }

        public PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
        {
            var conversionResult = new PdfConversionResult(documentId, PdfConverterType.AsposeHte);
            var pdfStream = new MemoryStream();

            try
            {
                var doc = _asposeItemFactory.CreateHtmlDocument(inputStream, correlationId);
                doc.Save(pdfStream);
                pdfStream.Seek(0, SeekOrigin.Begin);

                conversionResult.RecordConversionSuccess(pdfStream);
            }
            catch (UnsupportedFileFormatException ex)
            {
                inputStream?.Dispose();
                conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeHtmlUnsupportedFileFormat,
                    ex.ToFormattedString());
            }
            catch (IncorrectPasswordException ex)
            {
                inputStream?.Dispose();
                conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeHtmlPasswordProtected,
                    ex.ToFormattedString());
            }

            return conversionResult;
        }

        public Task<PdfConversionResult> ReadToPdfStreamAsync(Stream inputStream, string documentId, Guid correlationId)
        {
            throw new NotImplementedException();
        }
    }
}
