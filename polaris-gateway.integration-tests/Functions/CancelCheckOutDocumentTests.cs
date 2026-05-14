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
        var materialId = "CMS-12345";
        var documentId = 1;

        //act
        var result = await PolarisGatewayApiClient.CancelCheckoutDocumentAsync(urn, caseId, materialId, documentId,
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
        var materialId = "CMS-0";
        var documentId = 1;

        //act
        var result = await PolarisGatewayApiClient.CancelCheckoutDocumentAsync(urn, caseId, materialId, documentId,
            TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CancelCheckOutDocument_ShouldReturnOk()
    {
        //arrange
        var urn = "54KR7689125";
        var caseId = 2160797;
        var materialId = "CMS-8930494";
        var documentId = 8155871;
        var preCheckOutDocument = await PolarisGatewayApiClient.CheckOutDocumentAsync(urn, caseId, materialId, documentId, TestContext.CurrentContext.CancellationToken);

        //act
        var result = await PolarisGatewayApiClient.CancelCheckoutDocumentAsync(urn, caseId, materialId, documentId,
            TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(preCheckOutDocument.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}