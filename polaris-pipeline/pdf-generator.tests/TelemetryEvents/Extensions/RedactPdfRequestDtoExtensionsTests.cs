using System.Collections.Generic;
using Common.Dto.Request;
using Common.Dto.Request.Redaction;
using pdf_generator.TelemetryEvents.Extensions;
using Xunit;

namespace pdf_generator.tests.TelemetryEvents.Extensions
{
  public class RedactPdfRequestDtoHelperTests
  {
    public void RedactionPageCounts_ReturnsPageIndicesAndCounts()
    {
      // Arrange
      var redactPdfRequestDto = new RedactPdfRequestDto
      {
        RedactionDefinitions = new List<RedactionDefinitionDto> {
          new RedactionDefinitionDto {
            PageIndex = 0,
            RedactionCoordinates = new List<RedactionCoordinatesDto> {
              new RedactionCoordinatesDto(),
              new RedactionCoordinatesDto()
            }
          },
          new RedactionDefinitionDto {
            PageIndex = 1,
            RedactionCoordinates = new List<RedactionCoordinatesDto> {
              new RedactionCoordinatesDto()
            }
          }
        }
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