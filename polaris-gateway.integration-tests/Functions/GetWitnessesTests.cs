using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class GetWitnessesTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetWitnesses_CaseIdIs0_ShouldReturnNotFoundResult()
    {
        //arrange
        var urn = "urn";
        var caseId = 0;

        //act
        var result = await PolarisGatewayApiClient.GetWitnessesAsync(urn, caseId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task GetWitnesses_ShouldReturnWitnesses()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;

        //act
        var result = await PolarisGatewayApiClient.GetWitnessesAsync(urn, caseId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject.Count(), Is.EqualTo(4));
    }
}