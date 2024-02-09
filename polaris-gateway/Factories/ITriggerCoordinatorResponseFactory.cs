using Microsoft.AspNetCore.Http;
using PolarisGateway.Domain.PolarisPipeline;

namespace PolarisGateway.Factories
{
    public interface ITriggerCoordinatorResponseFactory
    {
        TriggerCoordinatorResponse Create(HttpRequest request, Guid correlationId);
    }
}

