using System;
using System.IO;
using System.Threading.Tasks;
using Aspose.Slides.Export;
using Aspose.Slides;
using pdf_generator.Domain.Document;
using pdf_generator.Factories.Contracts;
using Common.Constants;
using pdf_generator.Extensions;

namespace pdf_generator.Services.PdfService
{
    public class SlidesPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public SlidesPdfService(IAsposeItemFactory asposeItemFactory)
        {
            _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
        }

        public PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
        {
            var conversionResult = new PdfConversionResult(documentId, PdfConverterType.AsposeSlides);
            var pdfStream = new MemoryStream();

            try
            {
                using var presentation = _asposeItemFactory.CreatePresentation(inputStream, correlationId);
                presentation.Save(pdfStream, SaveFormat.Pdf);
                pdfStream.Seek(0, SeekOrigin.Begin);

                conversionResult.RecordConversionSuccess(pdfStream);
            }
            catch (InvalidPasswordException ex)
            {
                inputStream?.Dispose();
                conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeSlidesPasswordProtected, ex.ToFormattedString());
            }

            return conversionResult;
        }

        public Task<PdfConversionResult> ReadToPdfStreamAsync(Stream inputStream, string documentId, Guid correlationId)
        {
            throw new NotImplementedException();
        }
    }
}
