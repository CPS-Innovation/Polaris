using System.Net;
using NUnit.Framework;

namespace polaris_gateway.integration_tests.Functions;

public class GetCaseTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetCase_CaseIdIs0_ShouldReturnNotFound()
    {
        //arrange
        var urn = "urn";
        var caseId = 0;

        //act
        var result = await PolarisGatewayApiClient.GetCaseAsync(urn, caseId, TestContext.CurrentContext.CancellationToken);
        
        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetCase_ShouldReturnOkWithCaseDto()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;

        //act
        var result = await PolarisGatewayApiClient.GetCaseAsync(urn, caseId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);
        Assert.That(result.ResponseObject.Id, Is.EqualTo(caseId));
        Assert.That(result.ResponseObject.UniqueReferenceNumber, Is.EqualTo(urn));
    }
}