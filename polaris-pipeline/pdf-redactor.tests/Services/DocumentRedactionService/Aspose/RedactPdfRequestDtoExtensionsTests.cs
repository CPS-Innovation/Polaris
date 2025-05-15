using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using pdf_redactor.Services.DocumentRedaction.Aspose;
using Xunit;

namespace pdf_redactor.tests.Services.DocumentRedaction.Aspose
{
  public class RedactPdfRequestDtoHelperTests
  {
    public void RedactionPageCounts_ReturnsPageIndicesAndCounts()
    {
      // Arrange
      var redactPdfRequestDto = new RedactPdfRequestDto
      {
        RedactionDefinitions = [
          new RedactionDefinitionDto {
            PageIndex = 0,
            RedactionCoordinates = [
              new RedactionCoordinatesDto(),
              new RedactionCoordinatesDto()
            ]
          },
          new RedactionDefinitionDto {
            PageIndex = 1,
            RedactionCoordinates = [
              new RedactionCoordinatesDto()
            ]
          }
        ]
      };

      // Act
      var result = RedactPdfRequestDtoExtensions.RedactionPageCounts(redactPdfRequestDto);

      // Assert
      Assert.Equal(new Dictionary<int, int> {
        { 0, 2 },
        { 1, 1 }
      }, result);
    }
  }
}
