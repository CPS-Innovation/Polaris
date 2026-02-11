using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class GetExhibitProducersTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetExhibitProducers_CaseIdIs0_ShouldReturnNotFound()
    {
        //arrange
        var urn = "urn";
        var caseId = 0;
        //act
        var result = await PolarisGatewayApiClient.GetExhibitProducersAsync(urn, caseId, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetExhibitProducers_ReturnExhibitProducerList()
    {
        // Arrange
        var urn = "54KR7689125";
        var caseId = 2160797;

        // act
        var result = await PolarisGatewayApiClient.GetExhibitProducersAsync(urn, caseId, TestContext.CurrentContext.CancellationToken);

        // assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject, Is.Not.Null);
        Assert.That(result.ResponseObject.Count(), Is.AtLeast(3));
    }
}