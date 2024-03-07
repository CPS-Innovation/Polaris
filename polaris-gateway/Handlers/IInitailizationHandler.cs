using Microsoft.AspNetCore.Http;

namespace PolarisGateway.Handlers;

public interface IInitializationHandler
{
    public Task<(Guid, string)> Initialize(HttpRequest req);
}