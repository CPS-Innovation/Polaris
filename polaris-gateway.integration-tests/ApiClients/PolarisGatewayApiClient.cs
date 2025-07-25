using Common.Dto.Request;
using Common.Dto.Response.Case;
using NUnit.Framework;
using shared.integration_tests.ApiClients;
using shared.integration_tests.Models;
using System.Text.Json;
using Ddei.Domain.Response;

namespace polaris_gateway.integration_tests.ApiClients;

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

    public async Task<ApiClientResponse> CheckOutDocumentAsync(string urn, int caseId, string documentId, long versionId, CancellationToken cancellationToken = default)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/checkout";
        return await SendAsync(route, HttpMethod.Post, cancellationToken);
    }

    public async Task<ApiClientResponse> CancelCheckoutDocumentAsync(string urn, int caseId, string documentId, int versionId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/checkout";
        return await SendAsync(route, HttpMethod.Delete, cancellationToken);
    }

    public async Task<ApiClientResponse> AddDocumentNote(string urn, int caseId, string documentId, AddDocumentNoteRequestDto addDocumentNoteRequestDto, CancellationToken cancellationToken = default)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/notes";
        return await SendAsync(route, HttpMethod.Post, addDocumentNoteRequestDto, cancellationToken);
    }

    public async Task<ApiClientResponse<CaseDto>> GetCaseAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}";
        return await SendAsync<CaseDto>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<CaseDto>>> GetCases(string urn, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases";
        return await SendAsync<IEnumerable<CaseDto>>(route, HttpMethod.Get, cancellationToken);
    }

    private async Task<ApiClientResponse> SendAsync(string route, HttpMethod httpMethod, CancellationToken cancellationToken = default)
    {
        var token = await _tokenAuthApiClient.GetTokenAsync(cancellationToken);
        var cmsAuthValues = await _cmsAuthApiClient.GetCmsAuthTokenAsync(cancellationToken);
        var httpRequestMessage = CreateHttpRequestMessage(route, httpMethod, null, string.Empty, token, cmsAuthValues);
        var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken);
        return new ApiClientResponse(httpResponseMessage);
    }
    
    private async Task<ApiClientResponse> SendAsync<TRequest>(string route, HttpMethod httpMethod, TRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _tokenAuthApiClient.GetTokenAsync(cancellationToken);
        var cmsAuthValues = await _cmsAuthApiClient.GetCmsAuthTokenAsync(cancellationToken);
        var content = new StringContent(JsonSerializer.Serialize(request));
        var httpRequestMessage = CreateHttpRequestMessage(route, httpMethod, content,string.Empty, token, cmsAuthValues);
        var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken);
        return new ApiClientResponse(httpResponseMessage);
    }
    
    private async Task<ApiClientResponse<TResponse>> SendAsync<TResponse>(string route, HttpMethod httpMethod, CancellationToken cancellationToken = default)
    {
        var token = await _tokenAuthApiClient.GetTokenAsync(cancellationToken);
        var cmsAuthValues = await _cmsAuthApiClient.GetCmsAuthTokenAsync(cancellationToken);
        var httpRequestMessage = CreateHttpRequestMessage(route, httpMethod, null, string.Empty, token, cmsAuthValues);
        var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken);
        return new ApiClientResponse<TResponse>(httpResponseMessage);
    }
}