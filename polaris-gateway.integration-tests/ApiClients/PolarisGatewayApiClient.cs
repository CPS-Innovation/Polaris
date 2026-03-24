using Common.Dto.Request;
using Common.Dto.Response;
using Common.Dto.Response.Case;
using Common.Dto.Response.Document;
using Common.Dto.Response.Documents;
using NUnit.Framework;
using shared.integration_tests.ApiClients;
using shared.integration_tests.Models;
using System.Text.Json;
using Common.Domain.Ocr;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Response.HouseKeeping.Pcd;

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

    public async Task<ApiClientResponse<ExhibitProducersResponse>> GetCaseExhibitProducersAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/case-exhibit-producers";
        return await SendAsync<ExhibitProducersResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<CaseSummaryResponse>> GetCaseInfoAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/case-info/{caseId}";
        return await SendAsync<CaseSummaryResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<CaseMaterial>>> GetCaseMaterialsAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/case-materials";
        return await SendAsync<IEnumerable<CaseMaterial>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientFileResponse> GetCaseMaterialsPreviewAsync(
        string urn,
        int caseId,
        int materialId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/materials/{materialId}/preview";
        return await SendFileAsync(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<WitnessesResponse>> GetCaseWitnessesHkAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/case-witnesses";
        return await SendAsync<WitnessesResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<CaseLockedStatusResult>> GetCaseLockInfoAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/case-lock-info";
        return await SendAsync<CaseLockedStatusResult>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<DocumentTypeGroup>>> GetDocumentTypesAsync(
        string urn,
        int caseId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/document-types";
        return await SendAsync<IEnumerable<DocumentTypeGroup>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<object>> GetPiiAsync(
        string urn,
        int caseId,
        string documentId,
        long versionId,
        CancellationToken cancellationToken,
        bool? isOcrProcessed = null,
        bool? forceRefresh = null,
        Guid? token = null)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/pii";

        var qs = new List<string>();
        if (isOcrProcessed.HasValue) qs.Add($"isOcrProcessed={isOcrProcessed.Value.ToString().ToLowerInvariant()}");
        if (forceRefresh.HasValue) qs.Add($"ForceRefresh={forceRefresh.Value.ToString().ToLowerInvariant()}");
        if (token.HasValue) qs.Add($"token={token.Value}");

        if (qs.Count > 0)
        {
            route = $"{route}?{string.Join("&", qs)}";
        }

        return await SendAsync<object>(route, HttpMethod.Get, cancellationToken);
    }

    public record PiiPollResponse(string NextUrl);

    public async Task<ApiClientResponse<PiiPollResponse>> GetPiiPollAsync(
        string urn,
        int caseId,
        string documentId,
        long versionId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/pii";
        return await SendAsync<PiiPollResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientFileResponse> BulkRedactionSearchAsync(
        string urn,
        int caseId,
        string documentId,
        long versionId,
        string searchText,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/search";

        // Function reads req.Query["SearchText"]
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            route = $"{route}?SearchText={Uri.EscapeDataString(searchText)}";
        }

        return await SendFileAsync(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientFileResponse> CaseSearchAsync(
        string urn,
        int caseId,
        string query,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/search";

        if (!string.IsNullOrWhiteSpace(query))
        {
            route = $"{route}?query={Uri.EscapeDataString(query)}";
        }

        return await SendFileAsync(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientFileResponse> GetThumbnailAsync(
        string urn,
        int caseId,
        string documentId,
        int versionId,
        int maxDimensionPixel,
        int pageIndex,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/thumbnails/{maxDimensionPixel}/{pageIndex}";
        return await SendFileAsync(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<PcdRequestCore>>> GetPcdRequestCoreAsync(
        string urn,
        int caseId,
        int pcdId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/pcds/{pcdId}/pcd-request-core";
        return await SendAsync<IEnumerable<PcdRequestCore>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<PcdRequestDto>> GetPcdRequestAsync(
            string urn,
            int caseId,
            int pcdId,
            CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/pcds/{pcdId}/pcd-request";
        return await SendAsync<PcdRequestDto>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientFileResponse> GetMaterialDocumentAsync(
            string urn,
            int caseId,
            int materialId,
            CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/materials/{materialId}/document";
        return await SendFileAsync(route, HttpMethod.Get, cancellationToken);
    }

    public record OcrPollResponse(string NextUrl);

    public async Task<ApiClientResponse<OcrPollResponse>> GetOcrPollAsync(
        string urn,
        int caseId,
        string documentId,
        long versionId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/ocr";
        return await SendAsync<OcrPollResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<AnalyzeResults>> GetOcrAsync(
        string urn,
        int caseId,
        string documentId,
        long versionId,
        CancellationToken cancellationToken,
        bool? isOcrProcessed = null,
        bool? forceRefresh = null,
        Guid? token = null)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/ocr";

        // optional query string support (matches function query params)
        var qs = new List<string>();
        if (isOcrProcessed.HasValue) qs.Add($"isOcrProcessed={isOcrProcessed.Value.ToString().ToLowerInvariant()}");
        if (forceRefresh.HasValue) qs.Add($"ForceRefresh={forceRefresh.Value.ToString().ToLowerInvariant()}");
        if (token.HasValue) qs.Add($"token={token.Value}");

        if (qs.Count > 0)
        {
            route = $"{route}?{string.Join("&", qs)}";
        }

        return await SendAsync<AnalyzeResults>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<WitnessStatementsResponse>> GetCaseWitnessStatementsHkAsync(
            string urn,
            int caseId,
            int witnessId,
            CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/witnesses/{witnessId}/witness-statements";
        return await SendAsync<WitnessStatementsResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientFileResponse> GetPdfAsync(
        string urn,
        int caseId,
        string documentId,
        int versionId,
        CancellationToken cancellationToken,
        bool? isOcrProcessed = null,
        bool? forceRefresh = null)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/versions/{versionId}/pdf";
        return await SendFileAsync(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<DocumentDto>>> GetDocumentListAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents";
        return await SendAsync<IEnumerable<DocumentDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<DocumentNoteDto>>> GetDocumentNotesAsync(string urn, int caseId, string documentId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/notes";
        return await SendAsync<IEnumerable<DocumentNoteDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<ExhibitProducerDto>>> GetExhibitProducersAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/exhibit-producers";
        return await SendAsync<IEnumerable<ExhibitProducerDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<MaterialTypeDto>>> GetMaterialTypeListAsync(CancellationToken cancellationToken)
    {
        var route = "reference/reclassification";
        return await SendAsync<IEnumerable<MaterialTypeDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<CaseWitnessDto>>> GetWitnessesAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/witnesses";
        return await SendAsync<IEnumerable<CaseWitnessDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<WitnessStatementDto>>> GetWitnessStatementsAsync(string urn, int caseId, int witnessId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/witnesses/{witnessId}/statements";
        return await SendAsync<IEnumerable<WitnessStatementDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<CaseIdentifiersDto>> LookupUrnAsync(int caseId, CancellationToken cancellationToken)
    {
        var route = $"urn-lookup/{caseId}";
        return await SendAsync<CaseIdentifiersDto>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<DocumentReclassifiedResultDto>> ReclassifyDocumentAsync(int urn, int caseId, string documentId, ReclassifyDocumentDto request, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/reclassify";
        return await SendAsync<ReclassifyDocumentDto, DocumentReclassifiedResultDto>(route, HttpMethod.Post, request, cancellationToken);
    }

    public async Task<ApiClientResponse<PcdReviewDetailResponse>> GetPcdReviewDetailsAsync(
        string urn,
        int caseId,
        int historyId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/history/{historyId}/pcd-review-details";
        return await SendAsync<PcdReviewDetailResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse> RenameDocumentAsync(string urn, int caseId, string documentId, RenameDocumentRequestDto request, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{documentId}/rename";
        return await SendAsync<RenameDocumentRequestDto>(route, HttpMethod.Put, request, cancellationToken);
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
        var httpRequestMessage = CreateHttpRequestMessage(route, httpMethod, content, string.Empty, token, cmsAuthValues);
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

    private async Task<ApiClientFileResponse> SendFileAsync(
        string route,
        HttpMethod httpMethod,
        CancellationToken cancellationToken = default)
    {
        var token = await _tokenAuthApiClient.GetTokenAsync(cancellationToken);
        var cmsAuthValues = await _cmsAuthApiClient.GetCmsAuthTokenAsync(cancellationToken);

        var httpRequestMessage = CreateHttpRequestMessage(route, httpMethod, null, string.Empty, token, cmsAuthValues);
        var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken);

        var bytes = await httpResponseMessage.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = httpResponseMessage.Content.Headers.ContentType?.MediaType;
        var fileName = httpResponseMessage.Content.Headers.ContentDisposition?.FileName?.Trim('"');

        return new ApiClientFileResponse(httpResponseMessage.StatusCode, bytes, contentType, fileName);
    }

    private async Task<ApiClientResponse<TResponse>> SendAsync<TRequest, TResponse>(string route, HttpMethod httpMethod, TRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _tokenAuthApiClient.GetTokenAsync(cancellationToken);
        var cmsAuthValues = await _cmsAuthApiClient.GetCmsAuthTokenAsync(cancellationToken);
        var content = new StringContent(JsonSerializer.Serialize(request));
        var httpRequestMessage = CreateHttpRequestMessage(route, httpMethod, content, string.Empty, token, cmsAuthValues);
        var httpResponseMessage = await SendAsync(httpRequestMessage, cancellationToken);
        return new ApiClientResponse<TResponse>(httpResponseMessage);
    }

}