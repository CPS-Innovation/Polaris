using Common.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.common.Mappers.Contracts;
using PolarisGateway.Domain.PolarisPipeline;
using PolarisGateway.Factories.Contracts;

namespace PolarisGateway.Factories
{
    public class TriggerCoordinatorResponseFactory : ITriggerCoordinatorResponseFactory
    {
        private readonly ITrackerUrlMapper _trackerUrlMapper;

        public TriggerCoordinatorResponseFactory(ITrackerUrlMapper trackerUrlMapper)
        {
            _trackerUrlMapper = trackerUrlMapper;
        }

        public TriggerCoordinatorResponse Create(HttpRequest request, Guid correlationId) =>
            new TriggerCoordinatorResponse
            {
                TrackerUrl = _trackerUrlMapper.Map(request, correlationId)
            };
    }
}

