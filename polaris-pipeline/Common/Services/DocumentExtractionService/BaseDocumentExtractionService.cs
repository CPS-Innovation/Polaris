using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Exceptions;
using Common.Factories.Contracts;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace Common.Services.DocumentExtractionService;

public abstract class BaseDocumentExtractionService
{
    private readonly ILogger _logger;
    private readonly IHttpRequestFactory _httpRequestFactory;
    private readonly HttpClient _httpClient;

    protected BaseDocumentExtractionService(ILogger logger, IHttpRequestFactory httpRequestFactory, HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpRequestFactory = httpRequestFactory ?? throw new ArgumentNullException(nameof(httpRequestFactory));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    protected async Task<HttpContent> GetHttpContentAsync(string requestUri, string cmsAuthValues, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(GetHttpContentAsync), $"RequestUri: {requestUri}");

        var request = _httpRequestFactory.CreateGet(requestUri, cmsAuthValues, correlationId);
        var response = await _httpClient.SendAsync(request);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            throw new HttpException(response.StatusCode, exception);
        }

        var result = response.Content;
        _logger.LogMethodExit(correlationId, nameof(GetHttpContentAsync), string.Empty);
        return result;
    }
}
