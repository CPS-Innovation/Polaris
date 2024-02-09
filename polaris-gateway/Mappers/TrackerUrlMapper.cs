using Microsoft.AspNetCore.Http;
using PolarisGateway.common.Mappers;

namespace PolarisGateway.Mappers
{
    public class TrackerUrlMapper : ITrackerUrlMapper
    {
        public TrackerUrlMapper() { }

        public Uri Map(HttpRequest request, Guid correlationId)
        {
            var builder = new UriBuilder();
            builder.Scheme = request.Scheme;
            builder.Host = request.Host.Host;
            if (request.Host.Port.HasValue)
            {
                builder.Port = request.Host.Port.Value;
            }
            builder.Path = $"{request.Path}/tracker";

            return builder.Uri;
        }
    }
}

