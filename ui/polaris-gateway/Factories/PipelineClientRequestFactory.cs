using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using RumpoleGateway.Domain.Logging;

namespace RumpoleGateway.Factories
{
	public class PipelineClientRequestFactory : IPipelineClientRequestFactory
    {
        private readonly ILogger<PipelineClientRequestFactory> _logger;

        public PipelineClientRequestFactory(ILogger<PipelineClientRequestFactory> logger)
        {
            _logger = logger;
        }

        public HttpRequestMessage CreateGet(string requestUri, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(CreateGet), requestUri);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationKeys.Bearer, accessToken);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            _logger.LogMethodExit(correlationId, nameof(CreateGet), string.Empty);
            return request;
        }
        
        public HttpRequestMessage CreateGet(string requestUri, string accessToken, string upstreamToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(CreateGet), requestUri);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationKeys.Bearer, accessToken);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            request.Headers.Add(HttpHeaderKeys.UpstreamToken, upstreamToken);
            _logger.LogMethodExit(correlationId, nameof(CreateGet), string.Empty);
            return request;
        }

        public HttpRequestMessage CreatePut(string requestUri, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(CreatePut), requestUri);
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationKeys.Bearer, accessToken);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            _logger.LogMethodExit(correlationId, nameof(CreatePut), string.Empty);
            return request;
        }
    }
}

