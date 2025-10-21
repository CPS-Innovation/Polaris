using NUnit.Framework;
using System.Net;

namespace polaris_gateway.integration_tests.Functions;

public class GetMaterialTypeListTests : BaseFunctionIntegrationTest
{

    [SetUp]
    public void SetUp()
    {
        BaseSetup(TestContext.Parameters);
    }

    [Test]
    public async Task GetMaterialTypeList_ShouldReturnMaterialTypeList()
    {
        //arrange

        //act
        var result = await PolarisGatewayApiClient.GetMaterialTypeListAsync(TestContext.CurrentContext.CancellationToken);

        //assert
        Assert.That(result.HttpStatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(result.ResponseObject.Count(), Is.EqualTo(68));
    }
}