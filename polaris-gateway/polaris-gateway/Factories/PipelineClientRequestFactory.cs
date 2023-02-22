using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Factories.Contracts;

namespace PolarisGateway.Factories
{
    public class PipelineClientRequestFactory : IPipelineClientRequestFactory
    {
        private readonly ILogger<PipelineClientRequestFactory> _logger;

        public PipelineClientRequestFactory(ILogger<PipelineClientRequestFactory> logger)
        {
            _logger = logger;
        }

        public HttpRequestMessage CreateGet(string requestUri, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(CreateGet), requestUri);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            _logger.LogMethodExit(correlationId, nameof(CreateGet), string.Empty);
            return request;
        }

        public HttpRequestMessage CreateGet(string requestUri, string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(CreateGet), requestUri);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);
            _logger.LogMethodExit(correlationId, nameof(CreateGet), string.Empty);
            return request;
        }

        public HttpRequestMessage CreatePut(string requestUri, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(CreatePut), requestUri);
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            _logger.LogMethodExit(correlationId, nameof(CreatePut), string.Empty);
            return request;
        }
    }
}

