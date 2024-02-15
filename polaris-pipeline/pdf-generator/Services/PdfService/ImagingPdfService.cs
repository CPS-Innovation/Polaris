using System;
using System.IO;
using Aspose.Imaging.CoreExceptions;
using Aspose.Imaging.FileFormats.Pdf;
using Aspose.Imaging.ImageOptions;
using Common.Extensions;
using pdf_generator.Domain.Document;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class ImagingPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public ImagingPdfService(IAsposeItemFactory asposeItemFactory)
        {
            _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
        }

        public PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
        {
            var conversionResult = new PdfConversionResult(documentId, PdfConverterType.AsposeImaging);
            var pdfStream = new MemoryStream();

            try
            {
                using var image = _asposeItemFactory.CreateImage(inputStream, correlationId);
                image.Save(pdfStream, new PdfOptions { PdfDocumentInfo = new PdfDocumentInfo() });
                pdfStream.Seek(0, System.IO.SeekOrigin.Begin);
            
                conversionResult.RecordConversionSuccess(pdfStream);
            }
            catch (ImageLoadException ex)
            {
                inputStream?.Dispose();
                conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeImagingCannotLoad,
                    ex.ToFormattedString());
            }
            
            return conversionResult;
        }
    }
}
