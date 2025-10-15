using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class LookupUrnTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void SetUp()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task LookupUrn_CaseIdIs0_ShouldReturnNotFound()
    {
        //arrange
        var caseId = 0;

        //act
        var result = await PolarisGatewayApiClient.LookupUrnAsync(caseId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task LookupUrn_ShouldReturnCaseIdentifiers()
    {
        //arrange
        var caseId = 1;

        //act
        var result = await PolarisGatewayApiClient.LookupUrnAsync(caseId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject.Id, Is.EqualTo(caseId));
        Assert.That(result.ResponseObject.Urn, Is.EqualTo("42MZ7238121"));
        Assert.That(result.ResponseObject.UrnRoot, Is.EqualTo("42MZ7238121"));
    }
}