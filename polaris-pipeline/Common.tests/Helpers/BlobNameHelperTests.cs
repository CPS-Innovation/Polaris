using Common.Helpers;
using FluentAssertions;
using Xunit;

namespace Common.Tests.Helpers;

public class BlobNameHelperTests
{
    [Theory]
    [InlineData(12345, "CMS-6789", 54321, BlobNameHelper.BlobType.Pdf, "12345/pdfs/CMS-6789-54321.pdf")]
    [InlineData(12345, "CMS-PCD-6789", 54321, BlobNameHelper.BlobType.Pdf, "12345/pdfs/CMS-PCD-6789-54321.pdf")]
    [InlineData(12345, "DAC-foo", 54321, BlobNameHelper.BlobType.Pdf, "12345/pdfs/DAC-foo-54321.pdf")]

    [InlineData(12345, "CMS-6789", 54321, BlobNameHelper.BlobType.Ocr, "12345/ocrs/CMS-6789-54321.json")]
    [InlineData(12345, "CMS-PCD-6789", 54321, BlobNameHelper.BlobType.Ocr, "12345/ocrs/CMS-PCD-6789-54321.json")]
    [InlineData(12345, "DAC-foo", 54321, BlobNameHelper.BlobType.Ocr, "12345/ocrs/DAC-foo-54321.json")]

    [InlineData(12345, "CMS-6789", 54321, BlobNameHelper.BlobType.Pii, "12345/pii/CMS-6789-54321.json")]
    [InlineData(12345, "CMS-PCD-6789", 54321, BlobNameHelper.BlobType.Pii, "12345/pii/CMS-PCD-6789-54321.json")]
    [InlineData(12345, "DAC-foo", 54321, BlobNameHelper.BlobType.Pii, "12345/pii/DAC-foo-54321.json")]
    public void PdfBlobNameHelper_ReturnsExpectedPdfBlobName(int caseId, string documentId, long versionId, BlobNameHelper.BlobType blobType, string expected)
    {
        // Act
        var result = BlobNameHelper.GetBlobName(caseId, documentId, versionId, blobType);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(12345, "6789", 54321, BlobNameHelper.BlobType.Pdf)]
    [InlineData(12345, "6789", 54321, BlobNameHelper.BlobType.Ocr)]
    [InlineData(12345, "6789", 54321, BlobNameHelper.BlobType.Pii)]
    public void PdfBlobNameHelper_NumericDocumentIdsShouldThrow_UntilWeChooseToRefactorBackToNumericIds(int caseId, string documentId, long versionId, BlobNameHelper.BlobType blobType)
    {
        // Act
        var act = () => BlobNameHelper.GetBlobName(caseId, documentId, versionId, blobType);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void PdfBlobNameHelper_GetSafePrefix_ReturnsExpectedPrefix()
    {
        // Arrange
        const int caseId = 12345;
        const string expected = "12345/";

        // Act
        var result = BlobNameHelper.GetSafePrefix(caseId);

        // Assert
        result.Should().Be(expected);
    }
}