using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetCaseMaterialsTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetCaseMaterials_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/case-materials
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetCaseMaterialsAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseMaterials_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseMaterialsAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseMaterials_ShouldReturnOk_WithListOfCaseMaterials()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseMaterialsAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);
    }

    [Test]
    public async Task GetCaseMaterials_ShouldReturnOK_AndValidateReadStatusIsNormalized()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseMaterialsAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var materials = result.ResponseObject!.ToList();

        // Function normalizes:
        // if ReadStatus == "Complete" => "Read" else => "Unread"
        var invalid = materials
            .Where(m => m != null)
            .Where(m =>
            {
                var rs = (m.ReadStatus ?? string.Empty).Trim();
                return rs != "Read" && rs != "Unread";
            })
            .ToList();

        Assert.That(invalid, Is.Empty, "Expected ReadStatus to be normalized to 'Read' or 'Unread' only.");
    }

    [Test]
    public async Task GetCaseMaterials_ShouldReturnOK_AndValidateNoDuplicateIds()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseMaterialsAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var materials = result.ResponseObject!.ToList();

        var duplicateIds = materials
            .GroupBy(m => m.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.That(duplicateIds, Is.Empty, "Expected CaseMaterial Id values to be unique.");
    }

    [Test]
    public async Task GetCaseMaterials_ShouldReturnOK_AndValidateKeyFieldsArePopulated_WhenItemsExist()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseMaterialsAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var materials = result.ResponseObject!.ToList();

        if (!materials.Any())
        {
            Assert.Pass("No materials returned for this case in this environment; skipping deep field validation.");
        }

        Assert.That(materials.All(m => m.Id > 0), Is.True, "Expected all materials to have Id > 0.");
        Assert.That(materials.All(m => m.MaterialId >= 0), Is.True, "Expected MaterialId to be present (>= 0).");
        Assert.That(materials.All(m => m.Category != null), Is.True, "Expected Category to be non-null.");
        Assert.That(materials.All(m => m.Type != null), Is.True, "Expected Type to be non-null.");
    }
}