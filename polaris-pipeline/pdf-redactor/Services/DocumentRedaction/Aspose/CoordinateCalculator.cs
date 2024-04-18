using System;
using Aspose.Pdf.Facades;
using Common.Dto.Request.Redaction;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace pdf_redactor.Services.DocumentRedaction.Aspose
{
    public class CoordinateCalculator : ICoordinateCalculator
    {
        private readonly ILogger<CoordinateCalculator> _logger;

        public CoordinateCalculator(ILogger<CoordinateCalculator> logger)
        {
            _logger = logger;
        }

        public RedactionCoordinatesDto CalculateRelativeCoordinates(double pageWidth, double pageHeight, int pageIndex, RedactionCoordinatesDto originatorCoordinates, PdfFileInfo targetPdfInfo, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(CalculateRelativeCoordinates), string.Empty);

            var pdfTranslatedCoordinates = new RedactionCoordinatesDto();
            var x1Cent = originatorCoordinates.X1 * 100 / pageWidth;
            var y1Cent = originatorCoordinates.Y1 * 100 / pageHeight;
            var x2Cent = originatorCoordinates.X2 * 100 / pageWidth;
            var y2Cent = originatorCoordinates.Y2 * 100 / pageHeight;

            var pdfWidth = targetPdfInfo.GetPageWidth(pageIndex);
            var pdfHeight = targetPdfInfo.GetPageHeight(pageIndex);

            pdfTranslatedCoordinates.X1 = pdfWidth / 100 * x1Cent;
            pdfTranslatedCoordinates.Y1 = pdfHeight / 100 * y1Cent;
            pdfTranslatedCoordinates.X2 = pdfWidth / 100 * x2Cent;
            pdfTranslatedCoordinates.Y2 = pdfHeight / 100 * y2Cent;

            _logger.LogMethodExit(correlationId, nameof(CalculateRelativeCoordinates), string.Empty);
            return pdfTranslatedCoordinates;
        }
    }
}
