using Common.Dto.Document;
using coordinator.Validators;
using FluentAssertions;
using Xunit;

namespace coordinator.tests.Validators;

public class CmsDocumentsResponseValidatorTests
{
    [Fact]
    public void CmsDocumentsResponseValidator_WhenNoDuplicatesArePresent_ReturnsTrue()
    {
        // Arrange
        var validator = new CmsDocumentsResponseValidator();

        var docs = new[] {
            new CmsDocumentDto {DocumentId = "1"},
            new CmsDocumentDto {DocumentId = "2"},
        };

        // Act
        var result = validator.Validate(docs);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CmsDocumentsResponseValidator_WhenDuplicatesArePresent_ReturnsFalse()
    {
        // Arrange
        var validator = new CmsDocumentsResponseValidator();

        var docs = new[] {
            new CmsDocumentDto {DocumentId = "1"},
            new CmsDocumentDto {DocumentId = "2"},
            new CmsDocumentDto {DocumentId = "1"},
        };

        // Act
        var result = validator.Validate(docs);

        // Assert
        result.Should().BeFalse();
    }
}