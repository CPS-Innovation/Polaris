using System;
using System.Linq;
using Aspose.Pdf;
using AutoFixture;
using polaris_common.Dto.Request;
using polaris_common.Dto.Request.Redaction;
using polaris_common.Telemetry.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using pdf_generator.Services.DocumentRedaction.Aspose;
using pdf_generator.Services.DocumentRedaction.Aspose.RedactionImplementations;
using Xunit;

namespace pdf_generator.tests.Services.DocumentRedaction.Aspose;

public class AsposeRedactionProviderIntegrationTests
{
  private readonly Guid _correlationId;
  private readonly AsposeRedactionProvider _asposeRedactionProvider;

  public AsposeRedactionProviderIntegrationTests()
  {
    var fixture = new Fixture();
    _correlationId = fixture.Create<Guid>();

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
  public void AsposeRedactionProvider_Redact_OnlyConvertsPagesWhereRedactionsOccur()
  {
    // Arrange
    var assertPageImageCount = (Page page, int expectedImagesCount) =>
    {
      var absorber = new ImagePlacementAbsorber();
      page.Accept(absorber);
      absorber.ImagePlacements.Should().HaveCount(expectedImagesCount);
    };

    using var inputStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.tests.TestResources.TestPdf.pdf");
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
    using var outputStream = _asposeRedactionProvider.Redact(inputStream, redactPdfRequestDto, _correlationId);
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