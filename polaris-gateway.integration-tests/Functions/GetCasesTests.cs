using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class GetCasesTests : BaseFunctionIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetCases_ShouldReturnOK()
    {
        //arrange 
        var urn = "54KR7689125";

        //act
        var result = await PolarisGatewayApiClient.GetCases(urn, TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject,Is.Not.Null);
        var responseObjectList = result.ResponseObject.ToList();
        Assert.That(responseObjectList.Count, Is.EqualTo(1));
        Assert.That(responseObjectList[0].UniqueReferenceNumber, Is.EqualTo(urn));
    }
}