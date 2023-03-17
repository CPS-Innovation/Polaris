using System;
using System.Net.Http;

namespace Common.Factories.Contracts
{
    public interface IPipelineClientRequestFactory
    {
        HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null);
    }
}

