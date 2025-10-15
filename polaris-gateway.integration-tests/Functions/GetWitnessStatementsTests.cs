using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class GetWitnessStatementsTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void SetUp()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetWitnessStatements_CaseIdIs0_ShouldReturnNotFoundResult()
    {
        //arrange
        var urn = "urn";
        var caseId = 0;
        var witnessId = 2;

        //act
        var result = await PolarisGatewayApiClient.GetWitnessStatementsAsync(urn, caseId, witnessId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task GetWitnessStatements_WitnessIdIs0_ShouldReturnBadRequestResult()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var witnessId = 0;

        //act
        var result = await PolarisGatewayApiClient.GetWitnessStatementsAsync(urn, caseId, witnessId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task GetWitnessStatements_ShouldReturnWitnessStatements()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;
        var witnessId = 2;

        //act
        var result = await PolarisGatewayApiClient.GetWitnessStatementsAsync(urn, caseId, witnessId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject.Count(), Is.EqualTo(1));
    }
}