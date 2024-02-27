using System;
using System.IO;
using Aspose.Cells;
using Common.Extensions;
using pdf_generator.Domain.Document;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class CellsPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public CellsPdfService(IAsposeItemFactory asposeItemFactory)
        {
            _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
        }

        public PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
        {
            var conversionResult = new PdfConversionResult(documentId, PdfConverterType.AsposeCells);
            var pdfStream = new MemoryStream();

            try
            {
                using var workbook = _asposeItemFactory.CreateWorkbook(inputStream, correlationId);
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
    }
}
