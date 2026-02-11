using Common.Dto.Request;
using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class ReclassifyDocumentTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void SetUp()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task ReclassifyDocument_CaseIdIs0_ShouldReturnNotFound()
    {
        //arrange
        var request = new ReclassifyDocumentDto();
        var urn = 1;
        var caseId = 0;
        var documentId = "CMS-12345";

        //act
        var result = await PolarisGatewayApiClient.ReclassifyDocumentAsync(urn, caseId, documentId, request, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task ReclassifyDocument_RequestIsInvalid_ShouldReturnBadRequest()
    {
        //arrange
        var request = new ReclassifyDocumentDto();
        var urn = 1;
        var caseId = 2;
        var documentId = "CMS-12345";

        //act
        var result = await PolarisGatewayApiClient.ReclassifyDocumentAsync(urn, caseId, documentId, request, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ReclassifyDocument_DocumentIdNotFoundInListDocument_ShouldReturnInternalServiceError()
    {
        //arrange
        var request = new ReclassifyDocumentDto()
        {
            DocumentTypeId = 1,
        };
        var urn = 1;
        var caseId = 2;
        var documentId = "CMS-12345";

        //act
        var result = await PolarisGatewayApiClient.ReclassifyDocumentAsync(urn, caseId, documentId, request, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }
    
    [Test]
    public async Task ReclassifyDocument_DocumentTypeIdNotFound_ShouldReturnInternalServiceError()
    {
        //arrange
        var request = new ReclassifyDocumentDto()
        {
            DocumentTypeId = -100,
        };
        var urn = 1;
        var caseId = 2;
        var documentId = "CMS-8834853";

        //act
        var result = await PolarisGatewayApiClient.ReclassifyDocumentAsync(urn, caseId, documentId, request, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
    }
}