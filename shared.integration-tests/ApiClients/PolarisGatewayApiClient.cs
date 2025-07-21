using NUnit.Framework;
using shared.integration_tests.Models;

namespace shared.integration_tests.ApiClients;

public class PolarisGatewayApiClient : BaseApiClient
{
    private readonly TokenAuthApiClient _tokenAuthApiClient;
    private readonly CmsAuthApiClient _cmsAuthApiClient;
    public PolarisGatewayApiClient(TestParameters configuration)
    {
        HttpClient = new HttpClient()
        {
            BaseAddress = new Uri(configuration["PolarisGatewayUri"]!)
        };
        _tokenAuthApiClient = new TokenAuthApiClient(configuration);
        _cmsAuthApiClient = new CmsAuthApiClient(configuration);
    }

    public async Task<ApiClientResponse> CheckOutDocumentAsync(string caseUrn, int caseId, string documentId, long versionId, CancellationToken cancellationToken = default)
    {
        var token = await _tokenAuthApiClient.GetTokenAsync(cancellationToken);
        var cmsAuthValues = await _cmsAuthApiClient.GetCmsAuthTokenAsync(cancellationToken);
        var httpRequestMessage = CreateHttpRequestMessage($"urns/{caseUrn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/checkout", HttpMethod.Post, string.Empty, token, cmsAuthValues);
        var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken);
        return new ApiClientResponse(httpResponseMessage);
    }
}