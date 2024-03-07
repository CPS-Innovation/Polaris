using System;
using System.Net.Http;

namespace coordinator.Factories
{
    public interface IPipelineClientRequestFactory
    {
        HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null);
    }
}

