using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

[TestFixture]
public class CheckOutDocumentTests() : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Category("IntegrationTests")]
    [Test]
    public async Task CheckoutDocument_ShouldReturn200()
    {
        //arrange
        var urn = "54KR7689125";
        var caseId = 2160797;
        var documentId = "CMS-8930494";
        var versionId = 8155871;

        //act
        var result = await PolarisGatewayApiClient.CheckOutDocumentAsync(urn, caseId, documentId, versionId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Category("IntegrationTests")]
    [Test]
    public async Task CheckoutDocument_CaseIdIs0_ShouldReturn404()
    {
        //arrange
        var urn = "urn";
        var caseId = 0;
        var documentId = "CMS-12345";
        var versionId = 1;

        //act
        var result = await PolarisGatewayApiClient.CheckOutDocumentAsync(urn, caseId, documentId, versionId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Category("IntegrationTests")]
    [Test]
    public async Task CheckoutDocument_DocumentIdIs0_ShouldReturn400()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-0";
        var versionId = 1;

        //act
        var result = await PolarisGatewayApiClient.CheckOutDocumentAsync(urn, caseId, documentId, versionId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Category("IntegrationTests")]
    [Test]
    public async Task CheckoutDocument_VersionIdIs0_ShouldReturn404()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 0;

        //act
        var result = await PolarisGatewayApiClient.CheckOutDocumentAsync(urn, caseId, documentId, versionId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Category("IntegrationTests")]
    [Test]
    public async Task CheckoutDocument_UrnIsCheckOut_ShouldReturn409()
    {
        //arrange
        var urn = "45AA4200123";
        var caseId = 2150969;
        var documentId = "CMS-8709092";
        var versionId = 7965216;

        //act
        var result = await PolarisGatewayApiClient.CheckOutDocumentAsync(urn, caseId, documentId, versionId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }
}