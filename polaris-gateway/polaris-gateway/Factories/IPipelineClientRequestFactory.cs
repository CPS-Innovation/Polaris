using System;
using System.Net.Http;

namespace PolarisGateway.Factories
{
    public interface IPipelineClientRequestFactory
    {
        HttpRequestMessage CreateGet(string requestUri, string accessToken, Guid correlationId);

        HttpRequestMessage CreateGet(string requestUri, string accessToken, string cmsAuthValues, Guid correlationId);

        HttpRequestMessage CreatePut(string requestUri, string accessToken, Guid correlationId);
    }
}

