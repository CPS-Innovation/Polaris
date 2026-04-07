using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetCaseWitnessStatementsTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetCaseWitnessStatements_CaseIdIs0_ShouldReturnNotFound()
    {
        // arrange
        // Route: urns/{caseUrn}/cases/{caseId:min(1)}/witnesses/{witnessId}/witness-statements
        // caseId=0 fails route constraint => NotFound
        var urn = "54KR7689125";
        var caseId = 0;
        var witnessId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetCaseWitnessStatementsHkAsync(
            urn,
            caseId,
            witnessId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseWitnessStatements_CaseUrnIsMissing_ShouldReturnNotFound()
    {
        // arrange
        var urn = "";
        var caseId = 2179140;
        var witnessId = 1;

        // act
        var result = await PolarisGatewayApiClient.GetCaseWitnessStatementsHkAsync(
            urn,
            caseId,
            witnessId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCaseWitnessStatements_WitnessIdIs0_ShouldReturnBadRequest()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;
        var witnessId = 0;

        // act
        var result = await PolarisGatewayApiClient.GetCaseWitnessStatementsHkAsync(
            urn,
            caseId,
            witnessId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetCaseWitnessStatements_ShouldReturnOk_WithWitnessStatementsResponse_ForARealWitness()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        var witnessesResult = await PolarisGatewayApiClient.GetCaseWitnessesHkAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        Assert.That(witnessesResult.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(witnessesResult.ResponseObject, Is.Not.Null);
        Assert.That(witnessesResult.ResponseObject!.Witnesses, Is.Not.Null);

        var witnessId = witnessesResult.ResponseObject!.Witnesses
            .Where(w => w.WitnessId.HasValue && w.WitnessId.Value > 0)
            .Select(w => w.WitnessId!.Value)
            .FirstOrDefault();

        if (witnessId < 1)
        {
            Assert.Pass("No witnesses returned for this case in this environment; skipping witness statements test.");
        }

        // act
        var result = await PolarisGatewayApiClient.GetCaseWitnessStatementsHkAsync(
            urn,
            caseId,
            witnessId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);

        var obj = result.ResponseObject!;
        Assert.That(obj.WitnessStatements, Is.Not.Null);

        // Optional, stable shape validation
        if (obj.WitnessStatements.Any())
        {
            Assert.That(obj.WitnessStatements.All(s => s.Id > 0), Is.True, "Expected statement Id > 0.");
            Assert.That(obj.WitnessStatements.All(s => s.StatementNumber > 0), Is.True, "Expected StatementNumber > 0.");
        }
    }

    [Test]
    public async Task GetCaseWitnessStatements_ShouldReturnOK_AndValidateNoDuplicateStatementIds_WhenPresent()
    {
        // arrange
        var urn = "16XL8836126";
        var caseId = 2179140;

        var witnessesResult = await PolarisGatewayApiClient.GetCaseWitnessesHkAsync(
            urn,
            caseId,
            TestContext.CurrentContext.CancellationToken);

        Assert.That(witnessesResult.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(witnessesResult.ResponseObject, Is.Not.Null);

        var witnessId = witnessesResult.ResponseObject!.Witnesses
            .Where(w => w.WitnessId.HasValue && w.WitnessId.Value > 0)
            .Select(w => w.WitnessId!.Value)
            .FirstOrDefault();

        if (witnessId < 1)
        {
            Assert.Pass("No witnesses returned for this case in this environment; skipping duplicate-id validation.");
        }

        // act
        var result = await PolarisGatewayApiClient.GetCaseWitnessStatementsHkAsync(
            urn,
            caseId,
            witnessId,
            TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));

        var statements = result.ResponseObject!.WitnessStatements ?? new();

        var duplicateIds = statements
            .GroupBy(s => s.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        Assert.That(duplicateIds, Is.Empty, "Expected statement Id values to be unique.");
    }
}