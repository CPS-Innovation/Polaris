namespace shared.integration_tests.ApiClients;

public interface IApiClient
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default);
}