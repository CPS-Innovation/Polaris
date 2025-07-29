using NUnit.Framework;
using polaris_gateway.integration_tests.ApiClients;
using shared.integration_tests.ApiClients;

namespace polaris_gateway.integration_tests.Functions;

[TestFixture]
public abstract class BaseFunctionIntegrationTest
{
    protected PolarisGatewayApiClient PolarisGatewayApiClient = null!;
    protected void BaseSetup(TestParameters configuration)
    {
        PolarisGatewayApiClient = new PolarisGatewayApiClient(configuration);
    }
}