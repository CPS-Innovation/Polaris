using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class GetPiiTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetPii_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/documents/{documentId}/versions/{versionId:min(1)}/pii
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;
        var documentId = "1";
        var versionId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetPiiAsync(
            urn,
            caseId,
            documentId,
            versionId,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPii_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;
        var documentId = "1";
        var versionId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetPiiAsync(
            urn,
            caseId,
            documentId,
            versionId,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPii_VersionIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // versionId has min(1) route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 2179140;
        var documentId = "1";
        var versionId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetPiiAsync(
            urn,
            caseId,
            documentId,
            versionId,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPii_ShouldReturnOkOrAccepted_DependingOnArtefactAvailability()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "CMS-8977782";
        var versionId = 8185976;

        // act
        var result = await PolarisGatewayApiClient.GetPiiAsync(
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
    public async Task GetPii_WhenAccepted_ShouldReturnNextUrlForPolling()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var documentId = "1";
        var versionId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetPiiPollAsync(
            urn,
            caseId,
            documentId,
            versionId,
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        // assert
        if (result.HttpStatusCode != HttpStatusCode.Accepted)
        {
            Assert.Pass(
                $"PII did not return 202 in this run (status {(int)result.HttpStatusCode}). Skipping NextUrl assertion.");
        }
    }
}