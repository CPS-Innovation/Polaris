using System.Runtime.InteropServices;
using coordinator.Helpers;
using FluentAssertions;
using Xunit;

namespace coordinator.Tests.Helpers;

public class PdfBlobNameHelperTests
{
    [Theory]
    [InlineData(12345, "6789", "12345/pdfs/CMS-6789.pdf")]
    [InlineData(12345, "CMS-6789", "12345/pdfs/CMS-6789.pdf")]
    [InlineData(12345, "DAC-foo", "12345/pdfs/CMS-DAC.pdf")]
    public void PdfBlobNameHelper_ReturnsExpectedPdfBlobName(int caseId, string cmsOrPolarisDocumentId, string expected)
    {
        // Act
        var result = PdfBlobNameHelper.GetPdfBlobName(caseId, cmsOrPolarisDocumentId);

        // Assert
        result.Should().Be(expected);
    }
}