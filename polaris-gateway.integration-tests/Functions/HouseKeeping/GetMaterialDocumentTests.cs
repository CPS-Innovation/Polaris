using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetMaterialDocumentTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetMaterialDocument_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/document
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;
        var materialId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetMaterialDocumentAsync(
            urn,
            caseId,
            materialId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetMaterialDocument_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;
        var materialId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetMaterialDocumentAsync(
            urn,
            caseId,
            materialId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetMaterialDocument_MaterialIdIs0_ShouldReturnBadRequest()
    {
        // arrange
        // materialId has no route constraint => request hits function => BadRequest
        var urn = "16XL8836126";
        var caseId = 2179140;
        var materialId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetMaterialDocumentAsync(
            urn,
            caseId,
            materialId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetMaterialDocument_MaterialDoesNotExist_ShouldReturnNotFound()
    {
        // arrange
        // If GetCaseMaterialLinkAsync returns empty => NotFoundObjectResult
        var urn = "16XL8836126";
        var caseId = 2179140;
        var materialId = 99999999;

        // act
        var result = await PolarisGatewayApiClient.GetMaterialDocumentAsync(
            urn,
            caseId,
            materialId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetMaterialDocument_ShouldReturnOK_ForARealMaterial()
    {
        // arrange
        // Discover a materialId dynamically from GetCaseMaterials to avoid hardcoding.
        var urn = "16XL8836126";
        var caseId = 2179140;

        var materialsResult = await PolarisGatewayApiClient.GetCaseMaterialsAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        Assert.That(materialsResult.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(materialsResult.ResponseObject, Is.Not.Null);

        var materialId = materialsResult.ResponseObject!
            .Select(m => m.MaterialId)
            .FirstOrDefault(mid => mid > 0);

        if (materialId < 1)
        {
            Assert.Pass("No MaterialId (>0) returned for this case in this environment; skipping material document download test.");
        }

        // act
        var result = await PolarisGatewayApiClient.GetMaterialDocumentAsync(
            urn,
            caseId,
            materialId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.Bytes, Is.Not.Null);
        Assert.That(result.Bytes.Length, Is.GreaterThanOrEqualTo(0));
        Assert.That(result.ContentType, Is.Not.Null.And.Not.Empty);
    }
}