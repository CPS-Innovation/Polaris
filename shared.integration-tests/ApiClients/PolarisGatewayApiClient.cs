using shared.integration_tests.Models;

namespace shared.integration_tests.ApiClients;

public class PolarisGatewayApiClient : BaseApiClient
{
    protected readonly TokenAuthApiClient TokenAuthApiClient;
    protected readonly CmsAuthApiClient CmsAuthApiClient;
    public PolarisGatewayApiClient()
    {
        HttpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:7075/api/")
        };
        TokenAuthApiClient = new TokenAuthApiClient();
        CmsAuthApiClient = new CmsAuthApiClient();
    }

    public async Task<ApiClientResponse> CheckOutDocumentAsync(string caseUrn, int caseId, string documentId, long versionId, CancellationToken cancellationToken = default)
    {
        var token = await TokenAuthApiClient.GetTokenAsync(cancellationToken);
        var cmsAuthValues = await CmsAuthApiClient.GetCmsAuthTokenAsync(cancellationToken);
        var httpRequestMessage = CreateHttpRequestMessage($"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/checkout", HttpMethod.Post, string.Empty, token, cmsAuthValues);
        var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken);
        return new ApiClientResponse(httpResponseMessage);
    }
}