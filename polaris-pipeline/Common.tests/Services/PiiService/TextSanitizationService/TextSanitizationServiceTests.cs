using Xunit;
using Common.Services.PiiService.TextSanitization;

namespace Common.tests.Services.PiiService.TextSanitization
{
  public class TextSanitizationServiceTests
  {
    private readonly TextSanitizationService _service;

    public TextSanitizationServiceTests()
    {
      _service = new TextSanitizationService();
    }

    [Fact]
    public void SanitizeText_RemovesAsciiCharsFromEndOfText()
    {
      // Arrange
      string input = "Gibson,";
      string expected = "Gibson";

      // Act
      string result = _service.SanitizeText(input);

      // Assert
      Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitizeText_RemovesAsciiCharsFromStartOfText()
    {
      // Arrange
      string input = ".Gibson";
      string expected = "Gibson";

      // Act
      string result = _service.SanitizeText(input);

      // Assert
      Assert.Equal(expected, result);
    }
    [Fact]
    public void SanitizeText_ReturnsInputWhenNoAsciiChars()
    {
      // Arrange
      string input = "Gibson";
      string expected = "Gibson";

      // Act
      string result = _service.SanitizeText(input);

      // Assert
      Assert.Equal(expected, result);
    }
  }
}
