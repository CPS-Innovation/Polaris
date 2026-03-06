
using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetCaseMaterialsPreviewTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetCaseMaterialsPreview_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/materials/{materialId}/preview
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;
        var materialId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetCaseMaterialsPreviewAsync(
            urn,
            caseId,
            materialId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseMaterialsPreview_MaterialIdIs0_ShouldReturnBadRequest()
    {
        // arrange
        // materialId is NOT route constrained (no min(1) in route),
        // so request reaches function and returns BadRequestObjectResult
        var urn = "16XL8836126";
        var caseId = 2179140;
        var materialId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetCaseMaterialsPreviewAsync(
            urn,
            caseId,
            materialId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetCaseMaterialsPreview_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = string.Empty;
        var caseId = 2179140;
        var materialId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetCaseMaterialsPreviewAsync(
            urn,
            caseId,
            materialId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseMaterialsPreview_MaterialDoesNotExist_ShouldReturnNotFound()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var materialId = 99999999;

        // act
        var result = await PolarisGatewayApiClient.GetCaseMaterialsPreviewAsync(
            urn,
            caseId,
            materialId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseMaterialsPreview_ShouldReturnOK_OrFileResult_ForARealMaterial()
    {
        // arrange
        // We discover a materialId dynamically from GetCaseMaterials so the test is stable across environments.
        var urn = "16XL8836126";
        var caseId = 2179140;

        var materialsResult = await PolarisGatewayApiClient.GetCaseMaterialsAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        Assert.That(materialsResult.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(materialsResult.ResponseObject, Is.Not.Null);

        var firstMaterialId = materialsResult.ResponseObject!
            .Select(m => m.MaterialId)
            .FirstOrDefault(mid => mid > 0);

        if (firstMaterialId < 1)
        {
            Assert.Pass("No MaterialId (>0) returned for this case in this environment; skipping preview download test.");
        }

        // act
        var previewResult = await PolarisGatewayApiClient.GetCaseMaterialsPreviewAsync(
            urn,
            caseId,
            firstMaterialId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(previewResult.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));

        Assert.That(previewResult.Bytes, Is.Not.Null);
        Assert.That(previewResult.Bytes.Length, Is.GreaterThanOrEqualTo(0));
        Assert.That(previewResult.ContentType, Is.Not.Null.And.Not.Empty);
    }
}