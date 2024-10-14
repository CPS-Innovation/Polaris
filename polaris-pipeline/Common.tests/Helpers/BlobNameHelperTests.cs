using Common.Helpers;
using FluentAssertions;
using Xunit;

namespace Common.Tests.Helpers;

public class BlobNameHelperTests
{
    [Theory]
    [InlineData(12345, "6789", BlobNameHelper.BlobType.Pdf, "12345/pdfs/CMS-6789.pdf")]
    [InlineData(12345, "CMS-6789", BlobNameHelper.BlobType.Pdf, "12345/pdfs/CMS-6789.pdf")]
    [InlineData(12345, "CMS-PCD-6789", BlobNameHelper.BlobType.Pdf, "12345/pdfs/CMS-PCD-6789.pdf")]
    [InlineData(12345, "DAC-foo", BlobNameHelper.BlobType.Pdf, "12345/pdfs/CMS-DAC.pdf")]

    [InlineData(12345, "6789", BlobNameHelper.BlobType.Ocr, "12345/ocrs/CMS-6789.json")]
    [InlineData(12345, "CMS-6789", BlobNameHelper.BlobType.Ocr, "12345/ocrs/CMS-6789.json")]
    [InlineData(12345, "CMS-PCD-6789", BlobNameHelper.BlobType.Ocr, "12345/ocrs/CMS-PCD-6789.json")]
    [InlineData(12345, "DAC-foo", BlobNameHelper.BlobType.Ocr, "12345/ocrs/CMS-DAC.json")]

    [InlineData(12345, "6789", BlobNameHelper.BlobType.Pii, "12345/pii/CMS-6789.json")]
    [InlineData(12345, "CMS-6789", BlobNameHelper.BlobType.Pii, "12345/pii/CMS-6789.json")]
    [InlineData(12345, "CMS-PCD-6789", BlobNameHelper.BlobType.Pii, "12345/pii/CMS-PCD-6789.json")]
    [InlineData(12345, "DAC-foo", BlobNameHelper.BlobType.Pii, "12345/pii/CMS-DAC.json")]
    public void PdfBlobNameHelper_ReturnsExpectedPdfBlobName(int caseId, string cmsOrInternalDocumentId, BlobNameHelper.BlobType blobType, string expected)
    {
        // Act
        var result = BlobNameHelper.GetBlobName(caseId, cmsOrInternalDocumentId, blobType);

        // Assert
        result.Should().Be(expected);
    }
}