using System;
using Aspose.Pdf.Facades;
using Common.Domain.Redaction;

namespace pdf_generator.Services.DocumentRedactionService
{
    public interface ICoordinateCalculator
    {
        RedactionCoordinates CalculateRelativeCoordinates(double pageWidth, double pageHeight, int pageIndex, RedactionCoordinates originatorCoordinates, PdfFileInfo targetPdfInfo, Guid correlationId);
    }
}
