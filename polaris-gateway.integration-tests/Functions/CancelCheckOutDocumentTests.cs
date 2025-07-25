using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class CancelCheckOutDocumentTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task CancelCheckOutDocument_CaseIdIs0_ShouldReturnNotFound()
    {
        //arrange
        var urn = "urn";
        var caseId = 0;
        var documentId = "CMS-12345";
        var versionId = 1;

        //act
        var result = await PolarisGatewayApiClient.CancelCheckoutDocumentAsync(urn, caseId, documentId, versionId,
            TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task CancelCheckOutDocument_DocumentIdIs0_ShouldReturnBadRequest()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-0";
        var versionId = 1;

        //act
        var result = await PolarisGatewayApiClient.CancelCheckoutDocumentAsync(urn, caseId, documentId, versionId,
            TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CancelCheckOutDocument_ShouldReturnOk()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 1;

        //act
        var result = await PolarisGatewayApiClient.CancelCheckoutDocumentAsync(urn, caseId, documentId, versionId,
            TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}