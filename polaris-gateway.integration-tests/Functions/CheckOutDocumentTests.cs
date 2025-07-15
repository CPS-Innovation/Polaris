using System.Net;

namespace polaris_gateway.integration_tests.Functions;


public class CheckOutDocumentTests : BaseIntegrationTest
{
    [Trait("Category", "IntegrationTests")]
    [Fact]
    public async Task CheckoutDocument_ShouldReturn200()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 1;
        //act
        var result = await PolarisGatewayApiClient.CheckOutDocumentAsync(urn, caseId, documentId, versionId);
        
        //assert
        Assert.Equal(HttpStatusCode.OK, result.HttpStatusCode);
    }

    [Trait("Category", "IntegrationTests")]
    [Fact]
    public async Task CheckoutDocument_CaseIdIs0_ShouldReturn404()
    {
        //arrange
        var urn = "urn";
        var caseId = 0;
        var documentId = "CMS-12345";
        var versionId = 1;
        //act
        var result = await PolarisGatewayApiClient.CheckOutDocumentAsync(urn, caseId, documentId, versionId);

        //assert
        Assert.Equal(HttpStatusCode.NotFound, result.HttpStatusCode);
    }

    [Trait("Category", "IntegrationTests")]
    [Fact]
    public async Task CheckoutDocument_DocumentIdIs0_ShouldReturn400()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-0";
        var versionId = 1;
        //act
        var result = await PolarisGatewayApiClient.CheckOutDocumentAsync(urn, caseId, documentId, versionId);

        //assert
        Assert.Equal(HttpStatusCode.BadRequest, result.HttpStatusCode);
    }

    [Trait("Category", "IntegrationTests")]
    [Fact]
    public async Task CheckoutDocument_VersionIdIs0_ShouldReturn404()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 0;
        //act
        var result = await PolarisGatewayApiClient.CheckOutDocumentAsync(urn, caseId, documentId, versionId);

        //assert
        Assert.Equal(HttpStatusCode.NotFound, result.HttpStatusCode);
    }

    [Trait("Category", "IntegrationTests")]
    [Fact]
    public async Task CheckoutDocument_UrnIsCheckOut_ShouldReturn409()
    {
        //arrange
        var urn = "urn-CheckedOut";
        var caseId = 1;
        var documentId = "CMS-12345";
        var versionId = 2;
        //act
        var result = await PolarisGatewayApiClient.CheckOutDocumentAsync(urn, caseId, documentId, versionId);

        //assert
        Assert.Equal(HttpStatusCode.Conflict, result.HttpStatusCode);
    }
}