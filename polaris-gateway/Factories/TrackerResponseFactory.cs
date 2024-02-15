using Microsoft.AspNetCore.Http;
using PolarisGateway.Domain.PolarisPipeline;

namespace PolarisGateway.Factories
{
    public class TrackerResponseFactory : ITrackerResponseFactory
    {
        public TrackerResponseFactory() { }

        public TriggerCoordinatorResponse Create(HttpRequest request, Guid correlationId)
        {
            var builder = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host
            };
            if (request.Host.Port.HasValue)
            {
                builder.Port = request.Host.Port.Value;
            }
            builder.Path = $"{request.Path}/tracker";

            var url = builder.Uri;
            return new TriggerCoordinatorResponse { TrackerUrl = url };
        }
    }
}

