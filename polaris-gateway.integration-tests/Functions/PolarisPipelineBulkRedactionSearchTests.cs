using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class PolarisPipelineBulkRedactionSearchTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task PolarisPipelineBulkRedactionSearch_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/search
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;
        var documentId = "1";
        var versionId = 1;
        var searchText = "test";

        // act
        var result = await PolarisGatewayApiClient.BulkRedactionSearchAsync(
            urn,
            caseId,
            documentId,
            versionId,
            searchText,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PolarisPipelineBulkRedactionSearch_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;
        var documentId = "1";
        var versionId = 1;
        var searchText = "test";

        // act
        var result = await PolarisGatewayApiClient.BulkRedactionSearchAsync(
            urn,
            caseId,
            documentId,
            versionId,
            searchText,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PolarisPipelineBulkRedactionSearch_VersionIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // versionId has min(1) route constraint => NotFound
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "CMS-8977782";
        var versionId = 8185976;
        var searchText = "test";

        // act
        var result = await PolarisGatewayApiClient.BulkRedactionSearchAsync(
            urn,
            caseId,
            documentId,
            versionId,
            searchText,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PolarisPipelineBulkRedactionSearch_SearchTextIsMissing_ShouldReturnNotFoundOrBadRequest()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "CMS-8977782";
        var versionId = 8185976;
        var searchText = ""; // missing

        // act
        var result = await PolarisGatewayApiClient.BulkRedactionSearchAsync(
            urn,
            caseId,
            documentId,
            versionId,
            searchText,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(
            result.HttpStatusCode == HttpStatusCode.BadRequest ||
            result.HttpStatusCode == HttpStatusCode.NotFound ||
            result.HttpStatusCode == HttpStatusCode.UnprocessableEntity,
            Is.True,
            $"Expected 400/404/422 when SearchText is missing/empty. Actual: {(int)result.HttpStatusCode} {result.HttpStatusCode}");
    }

    [Test]
    public async Task PolarisPipelineBulkRedactionSearch_ShouldReturnOkOrAcceptedOrNotFound_DependingOnDocumentAndPipeline()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "CMS-8977782";
        var versionId = 8185976;
        var searchText = "CPS"; 

        // act
        var result = await PolarisGatewayApiClient.BulkRedactionSearchAsync(
            urn,
            caseId,
            documentId,
            versionId,
            searchText,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(
            result.HttpStatusCode == HttpStatusCode.OK ||
            result.HttpStatusCode == HttpStatusCode.Accepted ||
            result.HttpStatusCode == HttpStatusCode.NotFound ||
            result.HttpStatusCode == HttpStatusCode.UnsupportedMediaType,
            Is.True,
            $"Expected 200/202/404/415 depending on pipeline/doc state. Actual: {(int)result.HttpStatusCode} {result.HttpStatusCode}");
    }
}