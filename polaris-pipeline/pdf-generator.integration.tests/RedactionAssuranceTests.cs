using Xunit;
using Moq;
using AutoFixture;
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
using Aspose.Pdf;

namespace pdf_generator_integration.tests
{
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
      new Aspose.Cells.License().SetLicense(licenceFileName);
      new Aspose.Diagram.License().SetLicense(licenceFileName);
      new Aspose.Email.License().SetLicense(licenceFileName);
      new Aspose.Imaging.License().SetLicense(licenceFileName);
      new Aspose.Pdf.License().SetLicense(licenceFileName);
      new Aspose.Slides.License().SetLicense(licenceFileName);
      new Aspose.Words.License().SetLicense(licenceFileName);

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

    [Theory]
    [InlineData("pdf_generator.integration.tests.TestResources.document.pdf", "pdf_generator.integration.tests.TestResources.document_redactions.json", "pdf_generator.integration.tests.TestResources.redacted_page_1.png", "pdf_generator.integration.tests.TestResources.redacted_page_2.png")]
    [InlineData("pdf_generator.integration.tests.TestResources.overlapping_redaction.pdf", "pdf_generator.integration.tests.TestResources.overlapping_redaction_redactions.json", "pdf_generator.integration.tests.TestResources.overlapping_redaction_page_1.png", null)]
    [InlineData("pdf_generator.integration.tests.TestResources.broken_ocr.pdf", "pdf_generator.integration.tests.TestResources.broken_ocr_redactions.json", "pdf_generator.integration.tests.TestResources.broken_ocr_page_1.png", null)]
    public async Task RedactionAssurance(string pdfResourcePath, string redactionJsonPath, string assertionImageOnePath, string assertionImageTwoPath)
    {
      // Arrange
      using var inputStream = GetType().Assembly.GetManifestResourceStream(pdfResourcePath) ?? throw new Exception($"{pdfResourcePath} not found");
      using var originalDocument = new Document(inputStream);

      var redactionData = LoadRedactionDataFromJson(originalDocument.FileName, redactionJsonPath);

      // Act
      using var outputStream = await _asposeRedactionProvider.Redact(inputStream, _caseId, _documentId, redactionData, _correlationId);
      var redactedDocument = new Document(outputStream);

      var redactedImageStreams = await PdfConversionHelper.ConvertAndSavePdfToImages(redactedDocument);

      // Assert
      using var assertionImageStreamOne = GetType().Assembly.GetManifestResourceStream(assertionImageOnePath) ?? throw new Exception($"{assertionImageOnePath} not found");
      var pageOneDiff = ImageSharpCompare.CalcDiff(redactedImageStreams[0], assertionImageStreamOne, ResizeOption.Resize);
      pageOneDiff.AbsoluteError.Should().BeLessThan(1);

      if (!string.IsNullOrEmpty(assertionImageTwoPath))
      {
        using var assertionImageStreamTwo = GetType().Assembly.GetManifestResourceStream(assertionImageTwoPath) ?? throw new Exception($"{assertionImageTwoPath} not found");
        var pageTwoDiff = ImageSharpCompare.CalcDiff(redactedImageStreams[1], assertionImageStreamTwo, ResizeOption.Resize);
        pageTwoDiff.AbsoluteError.Should().BeLessThan(1);
      }
    }

    private RedactPdfRequestDto LoadRedactionDataFromJson(string fileName, string jsonPath)
    {
      using var redactedStream = GetType().Assembly.GetManifestResourceStream(jsonPath) ?? throw new Exception($"{jsonPath} not found");
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
}
