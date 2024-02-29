using Xunit;
using Moq;
using AutoFixture;
using Aspose.Pdf;
using pdf_generator.Services.DocumentRedaction.Aspose;
using pdf_generator.Services.DocumentRedaction.Aspose.RedactionImplementations;
using Common.Telemetry.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Codeuctivity.ImageSharpCompare;
using FluentAssertions;
using Common.Dto.Request.Redaction;
using Common.Dto.Request;

namespace pdf_generator_integration.tests;

public class RedactionAssuranceTests
{
  private Fixture _fixture;
  private readonly Guid _correlationId;
  private readonly string _caseId;
  private readonly string _documentId;
  private readonly AsposeRedactionProvider _asposeRedactionProvider;

  public RedactionAssuranceTests()
  {
    _fixture = new Fixture();
    _correlationId = _fixture.Create<Guid>();
    _caseId = _fixture.Create<string>();
    _documentId = _fixture.Create<string>();

    const string licenceFileName = "Aspose.Total.NET.lic";
    new License().SetLicense(licenceFileName);

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
  public async Task RedactionAssurance_ImagesNotLostAsync()
  {
    // Arrange
    using var inputStream = GetType().Assembly.GetManifestResourceStream("pdf_generator_integration.tests.TestResources.document.pdf") ?? throw new Exception("pdf_generator_integration.tests.TestResources.document.pdf not found");
    using var originalDocument = new Document(inputStream);

    var redactionData = LoadRedactionDataFromJson(originalDocument.FileName);

    // Act
    using var outputStream = await _asposeRedactionProvider.Redact(inputStream, _caseId, _documentId, redactionData, _correlationId);
    var redactedDocument = new Document(outputStream);

    var redactedImageStreams = await PdfConversionHelper.ConvertAndSavePdfToImages(redactedDocument);

    var pdfOutputPath = "/Users/rhysbridges/Documents/CPS/Polaris/polaris-pipeline/pdf-generator-integration.tests/TestResources/redacted_document.pdf";
    using var pdfFileStream = File.Create(pdfOutputPath);
    outputStream.CopyTo(pdfFileStream);

    for (int i = 0; i < redactedImageStreams.Count; i++)
    {
      var imagePath = $"/Users/rhysbridges/Documents/CPS/Polaris/polaris-pipeline/pdf-generator-integration.tests/TestResources/redacted_page_{i + 1}.png";
      using var fileStream = File.Create(imagePath);
      redactedImageStreams[i].CopyTo(fileStream);
    }

    // Assert
    using var assertionImageStreamOne = GetType().Assembly.GetManifestResourceStream("pdf_generator_integration.tests.TestResources.document_page_one.png") ?? throw new Exception("pdf_generator_integration.tests.TestResources.document_page_one.png not found");
    using var assertionImageStreamTwo = GetType().Assembly.GetManifestResourceStream("pdf_generator_integration.tests.TestResources.document_page_two.png") ?? throw new Exception("pdf_generator_integration.tests.TestResources.document_page_two.png not found");

    redactedImageStreams[0].Position = 0;
    assertionImageStreamOne.Position = 0;
    redactedImageStreams[1].Position = 0;

    var pageOneDiff = ImageSharpCompare.CalcDiff(redactedImageStreams[0], assertionImageStreamOne, ResizeOption.Resize);
    pageOneDiff.AbsoluteError.Should().BeLessThan(1);

    var pageTwoDiff = ImageSharpCompare.CalcDiff(redactedImageStreams[1], assertionImageStreamTwo, ResizeOption.Resize);
    pageTwoDiff.AbsoluteError.Should().BeLessThan(1);
  }

  private RedactPdfRequestDto LoadRedactionDataFromJson(string fileName)
  {
    using var redactedStream = GetType().Assembly.GetManifestResourceStream("pdf_generator_integration.tests.TestResources.document_redactions.json") ?? throw new Exception("pdf_generator_integration.tests.TestResources.document_redactions.json not found");
    using var streamReader = new StreamReader(redactedStream);
    var jsonText = streamReader.ReadToEnd();
    var redactionData = JsonSerializer.Deserialize<RedactionData>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new Exception("Failed to deserialize redaction data");

    var redactionDefinitions = new List<RedactionDefinitionDto>();

    foreach (var redaction in redactionData.Redactions)
    {
      var redactionDefinition = new RedactionDefinitionDto
      {
        PageIndex = redaction.PageIndex,
        Width = redaction.Width,
        Height = redaction.Height,
        RedactionCoordinates = redaction.RedactionCoordinates.Select(rc => new RedactionCoordinatesDto
        {
          X1 = rc.X1,
          Y1 = rc.Y1,
          X2 = rc.X2,
          Y2 = rc.Y2
        }).ToList()
      };

      redactionDefinitions.Add(redactionDefinition);
    }

    return new RedactPdfRequestDto
    {
      FileName = fileName,
      VersionId = 1,
      RedactionDefinitions = redactionDefinitions
    };
  }
}
