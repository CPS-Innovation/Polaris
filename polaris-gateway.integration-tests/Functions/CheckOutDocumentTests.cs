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

        TestContext.Out.WriteLine(TestContext.Parameters["PolarisGatewayUri"]);
        TestContext.Out.WriteLine(TestContext.Parameters["CmsProxyUri"]);
        TestContext.Out.WriteLine(TestContext.Parameters["CmsUsername"]);
        TestContext.Out.WriteLine(TestContext.Parameters["CmsPassword"]);
        TestContext.Out.WriteLine(TestContext.Parameters["TokenAuthUri"]);
        TestContext.Out.WriteLine(TestContext.Parameters["ClientId"]);
        TestContext.Out.WriteLine(TestContext.Parameters["GrantType"]);
        TestContext.Out.WriteLine(TestContext.Parameters["Scope"]);
        TestContext.Out.WriteLine(TestContext.Parameters["ClientSecret"]);
        TestContext.Out.WriteLine(TestContext.Parameters["Username"]);
        TestContext.Out.WriteLine(TestContext.Parameters["Password"]);
    }

    [Category("IntegrationTests")]
    [Test]
    public async Task CheckoutDocument_ShouldReturn200()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 1;

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
        var urn = "urn-CheckedOut";
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 2;

        //act
        var result = await PolarisGatewayApiClient.CheckOutDocumentAsync(urn, caseId, documentId, versionId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }
}