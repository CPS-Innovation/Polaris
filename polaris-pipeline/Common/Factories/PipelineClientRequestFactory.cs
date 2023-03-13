using Common.Constants;
using Common.Factories.Contracts;
using Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;

namespace Common.Factories
{
    public class PipelineClientRequestFactory : IPipelineClientRequestFactory
    {
        private readonly ILogger<PipelineClientRequestFactory> _logger;

        public PipelineClientRequestFactory(ILogger<PipelineClientRequestFactory> logger)
        {
            _logger = logger;
        }

        public HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(Create), requestUri);
            var request = new HttpRequestMessage(httpMethod, requestUri);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            _logger.LogMethodExit(correlationId, nameof(Create), string.Empty);
            return request;
        }

        public HttpRequestMessage CreateAuthenticatedGet(string requestUri, string cmsAuthValues, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(CreateAuthenticatedGet), requestUri);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);
            _logger.LogMethodExit(correlationId, nameof(CreateAuthenticatedGet), string.Empty);
            return request;
        }
    }
}

