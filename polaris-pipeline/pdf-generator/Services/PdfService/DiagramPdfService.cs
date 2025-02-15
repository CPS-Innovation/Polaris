﻿using System;
using System.IO;
using System.Threading.Tasks;
// using Aspose.Diagram;
using pdf_generator.Domain.Document;
using pdf_generator.Factories.Contracts;

namespace pdf_generator.Services.PdfService
{
    public class DiagramPdfService : IPdfService
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public DiagramPdfService(IAsposeItemFactory asposeItemFactory)
        {
            _asposeItemFactory = asposeItemFactory ?? throw new ArgumentNullException(nameof(asposeItemFactory));
        }

        public PdfConversionResult ReadToPdfStream(Stream inputStream, string documentId, Guid correlationId)
        {
            throw new NotImplementedException("The conversion of diagram type documents is temporarily disabed as part of #28576");
            // var conversionResult = new PdfConversionResult(documentId, PdfConverterType.AsposeDiagrams);
            // var pdfStream = new MemoryStream();

            // var doc = _asposeItemFactory.CreateDiagram(inputStream, correlationId);
            // doc.Save(pdfStream, SaveFileFormat.Pdf);
            // pdfStream.Seek(0, SeekOrigin.Begin);

            // conversionResult.RecordConversionSuccess(pdfStream);
            // return conversionResult;
        }

        public Task<PdfConversionResult> ReadToPdfStreamAsync(Stream inputStream, string documentId, Guid correlationId)
        {
            throw new NotImplementedException();
        }
    }
}
