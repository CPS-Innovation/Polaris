using System;
using System.IO;
using Aspose.Cells;
using Aspose.Pdf.Facades;
using AutoFixture;
using Common.Dto.Request.Redaction;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Factories.Contracts;
using pdf_generator.Services.DocumentRedaction.Aspose;
using pdf_generator.Services.PdfService;
using Xunit;

namespace pdf_generator.tests.Services.DocumentRedaction.Aspose;

public class CoordinateCalculatorTests
{
    private readonly IPdfService _pdfService;
    private readonly Mock<IAsposeItemFactory> _asposeItemFactory;
    private readonly ICoordinateCalculator _coordinateCalculator;

    public CoordinateCalculatorTests()
    {
        var loggerMock = new Mock<ILogger<CoordinateCalculator>>();

        _asposeItemFactory = new Mock<IAsposeItemFactory>();
        _asposeItemFactory.Setup(x => x.CreateWorkbook(It.IsAny<Stream>(), It.IsAny<Guid>())).Returns(new Workbook());

        _pdfService = new CellsPdfService(_asposeItemFactory.Object);

        _coordinateCalculator = new CoordinateCalculator(loggerMock.Object);
    }

    [Fact]
    public void CalculateRelativeCoordinates_BasedOnTargetPdfDimensions()
    {
        using var inputStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.tests.TestResources.TestBook.xlsx");

        var conversionResult = _pdfService.ReadToPdfStream(inputStream, "test-document-id", Guid.NewGuid());

        var pdfInfo = new PdfFileInfo(conversionResult.ConvertedDocument);

        var fixture = new Fixture();
        var testCoordinates = fixture.Create<RedactionCoordinatesDto>();

        var targetPdfWidth = pdfInfo.GetPageWidth(1);
        var targetPdfHeight = pdfInfo.GetPageHeight(1);

        var x1Cent = testCoordinates.X1 * 100 / targetPdfWidth;
        var y1Cent = testCoordinates.Y1 * 100 / targetPdfHeight;
        var x2Cent = testCoordinates.X2 * 100 / targetPdfWidth;
        var y2Cent = testCoordinates.Y2 * 100 / targetPdfHeight;

        var testCalculation = _coordinateCalculator.CalculateRelativeCoordinates(targetPdfWidth, targetPdfHeight, 1, testCoordinates,
            pdfInfo, fixture.Create<Guid>());

        using (new AssertionScope())
        {
            _asposeItemFactory.Verify(x => x.CreateWorkbook(It.IsAny<Stream>(), It.IsAny<Guid>()));
            conversionResult.ConvertedDocument.Should().NotBeNull();
            conversionResult.ConvertedDocument.Length.Should().BeGreaterThan(0);

            testCalculation.X1.Should().Be(targetPdfWidth / 100 * x1Cent);
            testCalculation.Y1.Should().Be(targetPdfHeight / 100 * y1Cent);
            testCalculation.X2.Should().Be(targetPdfWidth / 100 * x2Cent);
            testCalculation.Y2.Should().Be(targetPdfHeight / 100 * y2Cent);
        }
    }
}
