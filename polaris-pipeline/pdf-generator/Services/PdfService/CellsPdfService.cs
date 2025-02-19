using System;
using System.IO;
using Aspose.Cells;
using pdf_generator.Extensions;
using pdf_generator.Domain.Document;
using pdf_generator.Factories.Contracts;
using System.Threading.Tasks;
using Common.Constants;

namespace pdf_generator.Services.PdfService
{
    public class CellsPdfService(IAsposeItemFactory asposeItemFactory) : IPdfService
    {
        public PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
        {
            var conversionResult = new PdfConversionResult(documentId, PdfConverterType.AsposeCells);
            var pdfStream = new MemoryStream();

            try
            {
                using var workbook = asposeItemFactory.CreateWorkbook(inputStream, correlationId);
                workbook.Save(pdfStream, new PdfSaveOptions { OnePagePerSheet = true });
                pdfStream.Seek(0, SeekOrigin.Begin);

                conversionResult.RecordConversionSuccess(pdfStream);
            }
            catch (CellsException ex)
            {
                inputStream?.Dispose();
                conversionResult.RecordConversionFailure(PdfConversionStatus.AsposeCellsGeneralError,
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
