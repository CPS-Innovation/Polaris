using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetCaseWitnessesHkTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetCaseWitnessesHk_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/case-witnesses
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetCaseWitnessesHkAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseWitnessesHk_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseWitnessesHkAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseWitnessesHk_ShouldReturnOk_WithWitnessesResponse()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseWitnessesHkAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;
        Assert.That(obj.Witnesses, Is.Not.Null);
    }

    [Test]
    public async Task GetCaseWitnessesHk_ShouldReturnOK_AndValidateWitnessesBelongToCase()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseWitnessesHkAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var witnesses = result.ResponseObject!.Witnesses ?? new();

        // If there are witnesses, they must match the caseId
        if (witnesses.Any())
        {
            Assert.That(
                witnesses.All(w => w.CaseId == caseId),
                Is.True,
                "Expected all returned witnesses to have CaseId matching the requested caseId.");
        }
        else
        {
            Assert.Pass("No witnesses returned for this case in this environment; skipping caseId consistency assertion.");
        }
    }

    [Test]
    public async Task GetCaseWitnessesHk_ShouldReturnOK_AndValidateWitnessIdsAndNames_WhenPresent()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        // act
        var result = await PolarisGatewayApiClient.GetCaseWitnessesHkAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var witnesses = result.ResponseObject!.Witnesses ?? new();

        if (!witnesses.Any())
        {
            Assert.Pass("No witnesses returned for this case in this environment; skipping witness field assertions.");
        }

        Assert.Multiple(() =>
        {
            // WitnessId is nullable in DTO, but in normal data it should be present
            Assert.That(witnesses.All(w => w.WitnessId.HasValue && w.WitnessId.Value > 0), Is.True, "Expected WitnessId to be present and > 0.");
            Assert.That(witnesses.All(w => !string.IsNullOrWhiteSpace(w.FirstName)), Is.True, "Expected FirstName to be populated.");
            Assert.That(witnesses.All(w => !string.IsNullOrWhiteSpace(w.Surname)), Is.True, "Expected Surname to be populated.");
        });

        // Uniqueness of WitnessId (when present)
        var duplicateIds = witnesses
            .Where(w => w.WitnessId.HasValue)
            .GroupBy(w => w.WitnessId!.Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.That(duplicateIds, Is.Empty, "Expected WitnessId values to be unique.");
    }
}