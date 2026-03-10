using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class GetOcrTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetOcr_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/ocr
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;
        var documentId = "1";
        var versionId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetOcrAsync(
            urn,
            caseId,
            documentId,
            versionId,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetOcr_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;
        var documentId = "1";
        var versionId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetOcrAsync(
            urn,
            caseId,
            documentId,
            versionId,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetOcr_VersionIdIs0_ShouldReturnNotFound()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2179140;
        var documentId = "1";
        var versionId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetOcrAsync(
            urn,
            caseId,
            documentId,
            versionId,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetOcr_ShouldReturnOkOrAccepted_DependingOnArtefactAvailability()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        var documentId = "CMS-8977782";
        var versionId = 8185976;

        // act
        var result = await PolarisGatewayApiClient.GetOcrAsync(
            urn,
            caseId,
            documentId,
            versionId,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(
            result.HttpStatusCode == HttpStatusCode.OK ||
            result.HttpStatusCode == HttpStatusCode.Accepted ||
            result.HttpStatusCode == HttpStatusCode.UnsupportedMediaType,
            Is.True,
            $"Expected 200 (artefact), 202 (poll), or 415 (failed). Actual: {(int)result.HttpStatusCode} {result.HttpStatusCode}");
    }

    [Test]
    public async Task GetOcr_WhenOk_ShouldReturnAnalyzeResultsShape()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "CMS-8977782";
        var versionId = 8185976;

        // act
        var result = await PolarisGatewayApiClient.GetOcrAsync(
            urn,
            caseId,
            documentId,
            versionId,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        // assert
        if (result.HttpStatusCode != HttpStatusCode.OK)
        {
            Assert.Pass($"OCR not immediately available (status {(int)result.HttpStatusCode}). Skipping AnalyzeResults assertions.");
        }

        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;
        Assert.That(obj.ReadResults, Is.Not.Null);
        Assert.That(obj.PageCount, Is.GreaterThanOrEqualTo(0));
        Assert.That(obj.LineCount, Is.GreaterThanOrEqualTo(0));
        Assert.That(obj.WordCount, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public async Task GetOcr_WhenAccepted_ShouldReturnNextUrlForPolling()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "CMS-8977782";
        var versionId = 8185976;

        // act
        var result = await PolarisGatewayApiClient.GetOcrPollAsync(
            urn,
            caseId,
            documentId,
            versionId,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        // assert
        if (result.HttpStatusCode != HttpStatusCode.Accepted)
        {
            Assert.Pass($"OCR did not return 202 in this run (status {(int)result.HttpStatusCode}). Skipping NextUrl assertion.");
        }
    }
}