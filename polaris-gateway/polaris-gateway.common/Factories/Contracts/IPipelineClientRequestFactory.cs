using System;
using System.Net.Http;

namespace PolarisGateway.Factories.Contracts
{
    public interface IPipelineClientRequestFactory
    {
        HttpRequestMessage CreateGet(string requestUri, Guid correlationId);

        HttpRequestMessage CreateGet(string requestUri, string cmsAuthValues, Guid correlationId);

        HttpRequestMessage CreatePut(string requestUri, Guid correlationId);
    }
}

