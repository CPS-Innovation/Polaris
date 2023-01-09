using System;
using System.Net.Http;
using Common.Constants;
using Common.Factories.Contracts;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace Common.Factories;

public class HttpRequestFactory : IHttpRequestFactory
{
    private readonly ILogger<HttpRequestFactory> _logger;

    public HttpRequestFactory(ILogger<HttpRequestFactory> logger)
    {
        _logger = logger;
    }

    public HttpRequestMessage CreateGet(string requestUri, string upstreamToken, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(CreateGet), $"RequestUri: {requestUri}");
            
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        
        request.Headers.Add(HttpHeaderKeys.Authorization, $"{AuthenticationKeys.Bearer} not-implemented-yet");
        request.Headers.Add(HttpHeaderKeys.UpstreamTokenName, upstreamToken);
        request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            
        _logger.LogMethodExit(correlationId, nameof(CreateGet), string.Empty);
        return request;
    }
}