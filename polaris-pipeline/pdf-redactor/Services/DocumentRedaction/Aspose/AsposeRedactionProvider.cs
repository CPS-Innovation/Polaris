using Aspose.Pdf;
using Aspose.Pdf.Facades;
using Common.Dto.Request;
using Color = System.Drawing.Color;

namespace pdf_redactor.Services.DocumentRedaction.Aspose;

public class AsposeRedactionProvider(ICoordinateCalculator coordinateCalculator) : IRedactionProvider
{
    public async Task<Stream> Redact(Stream stream, int caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId)
    {
        var document = new Document(stream);

        var editor = new PdfAnnotationEditor(document);
        foreach (var redactionPage in redactPdfRequest.RedactionDefinitions)
        {
            foreach (var coordinates in redactionPage.RedactionCoordinates)
            {
                var pdfInfo = new PdfFileInfo(document);
                var translatedCoordinates = coordinateCalculator.CalculateRelativeCoordinates(redactionPage.Width,
                    redactionPage.Height, redactionPage.PageIndex, coordinates, pdfInfo, correlationId);
                editor.RedactArea(redactionPage.PageIndex,
                        new Rectangle(translatedCoordinates.X1, translatedCoordinates.Y1, translatedCoordinates.X2, translatedCoordinates.Y2), Color.Black);
            }
        }

        var outputStream = new MemoryStream();
        editor.Save(outputStream);
        return outputStream;
    }
}