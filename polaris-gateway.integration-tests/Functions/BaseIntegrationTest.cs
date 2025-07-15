using shared.integration_tests.ApiClients;

namespace polaris_gateway.integration_tests.Functions;

public abstract class BaseIntegrationTest
{
    protected readonly PolarisGatewayApiClient PolarisGatewayApiClient;
    protected BaseIntegrationTest()
    {
        PolarisGatewayApiClient = new PolarisGatewayApiClient();
    }
}