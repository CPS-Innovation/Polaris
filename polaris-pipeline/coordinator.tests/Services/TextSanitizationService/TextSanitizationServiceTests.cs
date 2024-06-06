using System;
using Xunit;
using coordinator.Services.TextSanitizationService;

namespace coordinator.tests.Services.TextSanitization
{
  public class TextSanitizationServiceTests
  {
    private readonly TextSanitizationService _service;

    public TextSanitizationServiceTests()
    {
      _service = new TextSanitizationService();
    }

    [Fact]
    public void SantitizeText_RemovesAsciiCharsFromEndOfText()
    {
      // Arrange
      string input = "Gibson,";
      string expected = "Gibson";

      // Act
      string result = _service.SantitizeText(input);

      // Assert
      Assert.Equal(expected, result);
    }

    [Fact]
    public void SantitizeText_RemovesAsciiCharsFromStartOfText()
    {
      // Arrange
      string input = ".Gibson";
      string expected = "Gibson";

      // Act
      string result = _service.SantitizeText(input);

      // Assert
      Assert.Equal(expected, result);
    }
    [Fact]
    public void SantitizeText_RemovesAsciiCharsWithinText()
    {
      // Arrange
      string input = "Gibson's";
      string expected = "Gibsons";

      // Act
      string result = _service.SantitizeText(input);

      // Assert
      Assert.Equal(expected, result);
    }
    [Fact]
    public void SantitizeText_ReturnsInputWhenNoAsciiChars()
    {
      // Arrange
      string input = "Gibson";
      string expected = "Gibson";

      // Act
      string result = _service.SantitizeText(input);

      // Assert
      Assert.Equal(expected, result);
    }
  }
}
