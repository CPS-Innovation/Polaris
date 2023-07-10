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

        public HttpRequestMessage Create(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null)
        {
            _logger.LogMethodEntry(correlationId, nameof(Create), requestUri);

            var request = new HttpRequestMessage(httpMethod, requestUri);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            if (cmsAuthValues != null)
                request.Headers.Add(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);

            _logger.LogMethodExit(correlationId, nameof(Create), string.Empty);
            return request;
        }
    }
}

