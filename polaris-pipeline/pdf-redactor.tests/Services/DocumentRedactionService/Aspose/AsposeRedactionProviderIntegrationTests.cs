using System;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Pdf;
using AutoFixture;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using Common.Telemetry;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using pdf_redactor.Services.DocumentRedaction.Aspose;
using pdf_redactor.Services.DocumentRedaction.Aspose.RedactionImplementations;
using Xunit;

namespace pdf_generator.tests.Services.DocumentRedaction.Aspose;

public class AsposeRedactionProviderIntegrationTests
{
  private readonly Guid _correlationId;
  private readonly string _caseId;
  private readonly string _documentId;
  private readonly AsposeRedactionProvider _asposeRedactionProvider;

  public AsposeRedactionProviderIntegrationTests()
  {
    var fixture = new Fixture();
    _correlationId = fixture.Create<Guid>();
    _caseId = fixture.Create<string>();
    _documentId = fixture.Create<string>();

    var redactionImplementation = new ImageConversionImplementation(
      Options.Create(new ImageConversionOptions
      {
        Resolution = 150,
        QualityPercent = 50
      }));

    var coordinateCalculator = new CoordinateCalculator(new Mock<ILogger<CoordinateCalculator>>().Object);

    _asposeRedactionProvider = new AsposeRedactionProvider(
      redactionImplementation,
      coordinateCalculator,
      new Mock<ITelemetryClient>().Object
    );
  }

  [Fact]
  public async Task AsposeRedactionProvider_Redact_OnlyConvertsPagesWhereRedactionsOccurAsync()
  {
    // Arrange
    var assertPageImageCount = (Page page, int expectedImagesCount) =>
    {
      var absorber = new ImagePlacementAbsorber();
      page.Accept(absorber);
      absorber.ImagePlacements.Should().HaveCount(expectedImagesCount);
    };

    using var inputStream = GetType().Assembly.GetManifestResourceStream("pdf_redactor.tests.TestResources.TestPdf.pdf");
    using var originalDocument = new Document(inputStream);

    // set up a redaction each on page 2 and page 4
    var redactPdfRequestDto = new RedactPdfRequestDto
    {
      RedactionDefinitions = new[] {
        new RedactionDefinitionDto {
          PageIndex = 2,
          Height = originalDocument.Pages[2].PageInfo.Height,
          Width = originalDocument.Pages[2].PageInfo.Width,
          RedactionCoordinates = new [] {
            new RedactionCoordinatesDto {
              X1 = 10,
              Y1 = 10,
              X2 = 20,
              Y2 = 20
            }
          }.ToList()
        },
        new RedactionDefinitionDto {
          PageIndex = 4,
          Height = originalDocument.Pages[4].PageInfo.Height,
          Width = originalDocument.Pages[4].PageInfo.Width,
          RedactionCoordinates = new [] {
            new RedactionCoordinatesDto {
              X1 = 10,
              Y1 = 10,
              X2 = 20,
              Y2 = 20
            }
          }.ToList()
        }
      }.ToList()
    };

    // Act
    using var outputStream = await _asposeRedactionProvider.Redact(inputStream, _caseId, _documentId, redactPdfRequestDto, _correlationId);
    using var redactedDocument = new Document(outputStream);

    // Assert
    // expect no images in our source document
    assertPageImageCount(originalDocument.Pages[1], 0);
    assertPageImageCount(originalDocument.Pages[2], 0);
    assertPageImageCount(originalDocument.Pages[3], 0);
    assertPageImageCount(originalDocument.Pages[4], 0);

    // expect an image each on redacted document on pages 2 and page 4
    assertPageImageCount(redactedDocument.Pages[1], 0);
    assertPageImageCount(redactedDocument.Pages[2], 1);
    assertPageImageCount(redactedDocument.Pages[3], 0);
    assertPageImageCount(redactedDocument.Pages[4], 1);
  }
}