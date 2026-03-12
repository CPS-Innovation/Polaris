using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetPcdRequestCoreTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetPcdRequestCore_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/pcds/{pcdId}/pcd-request-core
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;
        var pcdId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetPcdRequestCoreAsync(
            urn,
            caseId,
            pcdId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPcdRequestCore_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;
        var pcdId = 156619;

        // act
        var result = await PolarisGatewayApiClient.GetPcdRequestCoreAsync(
            urn,
            caseId,
            pcdId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPcdRequestCore_ShouldReturnOk_WithPcdRequestCoreCollection()
    {
        // arrange
        // Use your seeded dataset values that return at least 1 core item
        var urn = "16XL8836126";
        var caseId = 2179141;
        var pcdId = 156620;

        // act
        var result = await PolarisGatewayApiClient.GetPcdRequestCoreAsync(
            urn,
            caseId,
            pcdId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);
        Assert.That(result.ResponseObject!.Any(), Is.True);
    }

    [Test]
    public async Task GetPcdRequestCore_ShouldReturnOK_AndValidateData()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179141;
        var pcdId = 156620;

        // act
        var result = await PolarisGatewayApiClient.GetPcdRequestCoreAsync(
            urn,
            caseId,
            pcdId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var items = result.ResponseObject!.ToList();
        Assert.That(items.Count, Is.GreaterThanOrEqualTo(1));

        var duplicateIds = items
            .GroupBy(x => x.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.That(duplicateIds, Is.Empty);

        Assert.That(items.All(x => x.Id > 0), Is.True);
        Assert.That(items.All(x => !string.IsNullOrWhiteSpace(x.Type)), Is.True);
        Assert.That(items.All(x => !string.IsNullOrWhiteSpace(x.DecisionRequiredBy)), Is.True);
        Assert.That(items.All(x => !string.IsNullOrWhiteSpace(x.DecisionRequested)), Is.True);

        var core = items.SingleOrDefault(x => x.Id == pcdId);
        Assert.That(core, Is.Not.Null, $"Expected to find core item with id {pcdId} in response.");

        Assert.Multiple(() =>
        {
            Assert.That(core!.Id, Is.EqualTo(156620));
            Assert.That(core.Type, Is.EqualTo("Telephone"));
            Assert.That(core.DecisionRequiredBy, Is.EqualTo("2025-07-28"));
            Assert.That(core.DecisionRequested, Is.EqualTo("2025-07-18"));
        });
    }

    [Test]
    public async Task GetPcdRequestCore_ShouldReturnOK_AndValidateStableSeededValues_WhenKnown()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179141;
        var pcdId = 156620;

        // act
        var result = await PolarisGatewayApiClient.GetPcdRequestCoreAsync(
            urn,
            caseId,
            pcdId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var core = result.ResponseObject!.SingleOrDefault(x => x.Id == 156620);
        Assert.That(core, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(core!.Type, Is.EqualTo("Telephone"));
            Assert.That(core.DecisionRequiredBy, Is.EqualTo("2025-07-28"));
            Assert.That(core.DecisionRequested, Is.EqualTo("2025-07-18"));
        });
    }
}