using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class GetDocumentListTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetDocumentList_CaseIdIs0_ShouldReturnNotFound()
    {
        //arrange
        var urn = "urn";
        var caseId = 0;

        //act
        var result = await PolarisGatewayApiClient.GetDocumentListAsync(urn, caseId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetDocumentList_ShouldReturnOkWithDocumentDtoList()
    {
        //arrange
        var urn = "urn";
        var caseId = 1;

        //act
        var result = await PolarisGatewayApiClient.GetDocumentListAsync(urn, caseId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject.Count(), Is.EqualTo(17));
    }
}