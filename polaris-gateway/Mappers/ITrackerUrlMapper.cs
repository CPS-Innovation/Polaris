using Microsoft.AspNetCore.Http;

namespace PolarisGateway.common.Mappers
{
    public interface ITrackerUrlMapper
    {
        Uri Map(HttpRequest request, Guid correlationId);
    }
}

