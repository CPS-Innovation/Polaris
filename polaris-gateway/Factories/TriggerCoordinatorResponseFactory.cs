using Microsoft.AspNetCore.Http;
using PolarisGateway.common.Mappers;
using PolarisGateway.Domain.PolarisPipeline;

namespace PolarisGateway.Factories
{
    public class TriggerCoordinatorResponseFactory : ITriggerCoordinatorResponseFactory
    {
        private readonly ITrackerUrlMapper _trackerUrlMapper;

        public TriggerCoordinatorResponseFactory(ITrackerUrlMapper trackerUrlMapper)
        {
            _trackerUrlMapper = trackerUrlMapper;
        }

        public TriggerCoordinatorResponse Create(HttpRequest request, Guid correlationId)
        {
            var url = _trackerUrlMapper.Map(request, correlationId);
            return new TriggerCoordinatorResponse { TrackerUrl = url };
        }
    }
}

