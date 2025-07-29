using System.Net.Http.Headers;
using System.Net.Http.Json;
using shared.integration_tests.Models;

namespace shared.integration_tests.ApiClients;

public abstract class BaseApiClient : IApiClient
{
    protected HttpClient HttpClient = null!;

    public virtual async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default)
    {
        return await HttpClient.SendAsync(httpRequestMessage, cancellationToken);
    }

    protected virtual HttpRequestMessage CreateHttpRequestMessage<T>(string route, HttpMethod httpMethod, T requestObject, IDictionary<string, string>? headersDictionary = null)
    {
        var httpRequestMessage = CreateHttpRequestMessage(route, httpMethod, headersDictionary: headersDictionary);
        httpRequestMessage.Content = JsonContent.Create(requestObject);

        return httpRequestMessage;
    }

    protected virtual HttpRequestMessage CreateHttpRequestMessageWithForm(string route, HttpMethod httpMethod, IDictionary<string, string> form, IDictionary<string, string>? headersDictionary = null)
    {
        var httpRequestMessage = CreateHttpRequestMessage(route, httpMethod, null, string.Empty, null, string.Empty, headersDictionary);
        httpRequestMessage.Content = new FormUrlEncodedContent(form);

        return httpRequestMessage;
    }

    protected virtual HttpRequestMessage CreateHttpRequestMessage(string route, HttpMethod httpMethod, HttpContent? httpContent = null, string? functionKey = "", Token? token = null, string cmsAuthValues = "", IDictionary<string, string>? headersDictionary = null)
    {
        var httpRequestMessage = new HttpRequestMessage(httpMethod, route);

        if (headersDictionary != null)
            foreach (var header in headersDictionary)
                httpRequestMessage.Headers.Add(header.Key, header.Value);

        httpRequestMessage.Headers.Add("Correlation-Id", Guid.NewGuid().ToString());

        if (token is not null)
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

        if (!string.IsNullOrEmpty(functionKey))
            httpRequestMessage.Headers.Add("x-functions-key", functionKey);

        if (!string.IsNullOrEmpty(cmsAuthValues))
            httpRequestMessage.Headers.Add("Cookie", $"{cmsAuthValues};Path=/;Expires={DateTime.Today.AddDays(1):R};");

        httpRequestMessage.Content = httpContent;

        return httpRequestMessage;
    }
}