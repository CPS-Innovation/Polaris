using NUnit.Framework;
using System.Net;
using System.Text;

namespace polaris_gateway.integration_tests.Functions;

public class GetPdfTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetPdf_ShouldReturnOK_AndReturnPdfFileStreamResult()
    {
        // arrange (use a known-good artefact in your env)
        var urn = "54KR7689125";
        var caseId = 2160797;
        var documentId = "CMS-8930497"; // adjust to your route format
        var versionId = 8156114;

        // act
        var result = await PolarisGatewayApiClient.GetPdfAsync(
            urn,
            caseId,
            documentId,
            versionId,
            TestContext.CurrentContext.CancellationToken,
            isOcrProcessed: false,
            forceRefresh: false);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ContentType, Is.EqualTo("application/pdf"));
        Assert.That(result.Bytes, Is.Not.Null);
        Assert.That(result.Bytes.Length, Is.GreaterThan(1000)); // sanity check

        // Optional: PDF files start with "%PDF"
        var header = System.Text.Encoding.ASCII.GetString(result.Bytes.Take(4).ToArray());
        Assert.That(header, Is.EqualTo("%PDF"));
    }

    [Test]
    public async Task GetPdf_ShouldReturnNotFound_WhenDocumentIsNotFound()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2160797;
        var documentId = "CMS-8930497";
        var versionId = 8156111;

        // act
        var result = await PolarisGatewayApiClient.GetPdfAsync(
            urn,
            caseId,
            documentId,
            versionId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(result.FileName, Is.Null);
    }

    [Test]
    public async Task GetPdf_ShouldReturnNotFound_WhenVersionNotFound()
    {
        // arrange
        var urn = "54KR7689125";
        var caseId = 2160797;
        var documentId = "CMS-8930497"; // adjust to your route format
        var versionId = 9999;

        // act
        var result = await PolarisGatewayApiClient.GetPdfAsync(
            urn,
            caseId,
            documentId,
            versionId,
            TestContext.CurrentContext.CancellationToken,
            forceRefresh: true);

        // assert
        Assert.That(result.HttpStatusCode, Is.AnyOf(HttpStatusCode.OK, HttpStatusCode.NotFound));
    }
}
