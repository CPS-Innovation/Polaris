using System;
using Aspose.Pdf.Facades;
using Common.Dto.Request.Redaction;

namespace pdf_redactor.Services.DocumentRedaction.Aspose
{
    public interface ICoordinateCalculator
    {
        RedactionCoordinatesDto CalculateRelativeCoordinates(double pageWidth, double pageHeight, int pageIndex, RedactionCoordinatesDto originatorCoordinates, PdfFileInfo targetPdfInfo, Guid correlationId);
    }
}
