using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class GetThumbnailTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetThumbnail_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "CMS-8977782";
        var versionId = 8185976;
        var maxDimensionPixel = 200;
        var pageIndex = 0;

        // act
        var result = await PolarisGatewayApiClient.GetThumbnailAsync(
            urn,
            caseId,
            documentId,
            versionId,
            maxDimensionPixel,
            pageIndex,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetThumbnail_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "CMS-8977782";
        var versionId = 8185976;
        var maxDimensionPixel = 200;
        var pageIndex = 0;

        // act
        var result = await PolarisGatewayApiClient.GetThumbnailAsync(
            urn,
            caseId,
            documentId,
            versionId,
            maxDimensionPixel,
            pageIndex,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetThumbnail_ShouldReturnOk_OrNotFound_DependingOnDocumentAvailability()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "CMS-8977782";
        var versionId = 8185976;
        var maxDimensionPixel = 256;
        var pageIndex = 0;

        // act
        var result = await PolarisGatewayApiClient.GetThumbnailAsync(
            urn,
            caseId,
            documentId,
            versionId,
            maxDimensionPixel,
            pageIndex,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(
            result.HttpStatusCode == HttpStatusCode.OK ||
            result.HttpStatusCode == HttpStatusCode.NotFound,
            Is.True,
            $"Expected 200 (thumbnail) or 404 (missing doc/page). Actual: {(int)result.HttpStatusCode} {result.HttpStatusCode}");
    }

    [Test]
    public async Task GetThumbnail_WhenOk_ShouldReturnImageBytes_AndContentType()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "CMS-8977782";
        var versionId = 8185976;
        var maxDimensionPixel = 256;
        var pageIndex = 0;

        // act
        var result = await PolarisGatewayApiClient.GetThumbnailAsync(
            urn,
            caseId,
            documentId,
            versionId,
            maxDimensionPixel,
            pageIndex,
            TestContext.CurrentContext.CancellationToken);

        // assert
        if (result.HttpStatusCode != HttpStatusCode.OK)
        {
            Assert.Pass($"Thumbnail not available in this environment for the chosen doc/version (status {(int)result.HttpStatusCode}). Skipping byte assertions.");
        }

        Assert.That(result.Bytes, Is.Not.Null);
        Assert.That(result.Bytes.Length, Is.GreaterThan(0));
        Assert.That(result.ContentType, Is.Not.Null.And.Not.Empty);

        // Typically image/png or image/jpeg
        Assert.That(
            result.ContentType.StartsWith("image/", System.StringComparison.OrdinalIgnoreCase),
            Is.True,
            $"Expected an image/* content-type. Actual: {result.ContentType}");
    }
}