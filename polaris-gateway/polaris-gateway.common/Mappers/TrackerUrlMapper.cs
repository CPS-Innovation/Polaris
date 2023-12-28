using Microsoft.AspNetCore.Http;
using PolarisGateway.common.Mappers.Contracts;

namespace PolarisGateway.Mappers
{
    public class TrackerUrlMapper : ITrackerUrlMapper
    {
        public Uri Map(HttpRequest request, Guid correlationId) =>
            new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
                Path = $"{request.Path}/tracker",
                Port = request.Host.Port ?? 80
            }.Uri;
    }
}

