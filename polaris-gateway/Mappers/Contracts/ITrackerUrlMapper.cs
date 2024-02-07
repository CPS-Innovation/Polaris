using Microsoft.AspNetCore.Http;

namespace PolarisGateway.common.Mappers.Contracts
{
    public interface ITrackerUrlMapper
    {
        Uri Map(HttpRequest request, Guid correlationId);
    }
}

