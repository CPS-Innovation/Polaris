// <copyright file="PolarisGatewayApiClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace polaris_gateway.integration_tests.ApiClients;

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

public class PolarisGatewayApiClient : BaseApiClient
{
    private readonly TokenAuthApiClient tokenAuthApiClient;
    private readonly CmsAuthApiClient cmsAuthApiClient;

    public PolarisGatewayApiClient(TestParameters configuration)
    {
        this.HttpClient = new HttpClient()
        {
            BaseAddress = new Uri(configuration["PolarisGatewayUri"]!),
        };
        this.tokenAuthApiClient = new TokenAuthApiClient(configuration);
        this.cmsAuthApiClient = new CmsAuthApiClient(configuration);
    }

    public async Task<ApiClientResponse> CheckOutDocumentAsync(string urn, int caseId, string materialId, long documentId, CancellationToken cancellationToken = default)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/checkout";
        return await this.SendAsync(route, HttpMethod.Post, cancellationToken);
    }

    public async Task<ApiClientResponse> CancelCheckoutDocumentAsync(string urn, int caseId, string materialId, int documentId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/checkout";
        return await this.SendAsync(route, HttpMethod.Delete, cancellationToken);
    }

    public async Task<ApiClientResponse> AddDocumentNote(string urn, int caseId, string materialId, AddDocumentNoteRequestDto addDocumentNoteRequestDto, CancellationToken cancellationToken = default)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/notes";
        return await this.SendAsync(route, HttpMethod.Post, addDocumentNoteRequestDto, cancellationToken);
    }

    public async Task<ApiClientResponse<CaseDto>> GetCaseAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}";
        return await this.SendAsync<CaseDto>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<CaseDto>>> GetCases(string urn, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases";
        return await this.SendAsync<IEnumerable<CaseDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<ExhibitProducersResponse>> GetCaseExhibitProducersAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/case-exhibit-producers";
        return await this.SendAsync<ExhibitProducersResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<CaseSummaryResponse>> GetCaseInfoAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/case-info/{caseId}";
        return await this.SendAsync<CaseSummaryResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<CaseMaterial>>> GetCaseMaterialsAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/case-materials";
        return await this.SendAsync<IEnumerable<CaseMaterial>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientFileResponse> GetCaseMaterialsPreviewAsync(
        string urn,
        int caseId,
        int materialId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/materials/{materialId}/preview";
        return await this.SendFileAsync(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<WitnessesResponse>> GetCaseWitnessesHkAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/case-witnesses";
        return await this.SendAsync<WitnessesResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<CaseLockedStatusResult>> GetCaseLockInfoAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/case-lock-info";
        return await this.SendAsync<CaseLockedStatusResult>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<DocumentTypeGroup>>> GetDocumentTypesAsync(
        string urn,
        int caseId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/document-types";
        return await this.SendAsync<IEnumerable<DocumentTypeGroup>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<object>> GetPiiAsync(
        string urn,
        int caseId,
        string materialId,
        long documentId,
        CancellationToken cancellationToken,
        bool? isOcrProcessed = null,
        bool? forceRefresh = null,
        Guid? token = null)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/pii";

        var qs = new List<string>();
        if (isOcrProcessed.HasValue)
        {
            qs.Add($"isOcrProcessed={isOcrProcessed.Value.ToString().ToLowerInvariant()}");
        }

        if (forceRefresh.HasValue)
        {
            qs.Add($"ForceRefresh={forceRefresh.Value.ToString().ToLowerInvariant()}");
        }

        if (token.HasValue)
        {
            qs.Add($"token={token.Value}");
        }

        if (qs.Count > 0)
        {
            route = $"{route}?{string.Join("&", qs)}";
        }

        return await this.SendAsync<object>(route, HttpMethod.Get, cancellationToken);
    }

    public record PiiPollResponse(string NextUrl);

    public async Task<ApiClientResponse<PiiPollResponse>> GetPiiPollAsync(
        string urn,
        int caseId,
        string materialId,
        long documentId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/pii";
        return await this.SendAsync<PiiPollResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientFileResponse> BulkRedactionSearchAsync(
        string urn,
        int caseId,
        string materialId,
        long documentId,
        string searchText,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/search";

        // Function reads req.Query["SearchText"]
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            route = $"{route}?SearchText={Uri.EscapeDataString(searchText)}";
        }

        return await this.SendFileAsync(route, HttpMethod.Get, cancellationToken);
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

        return await this.SendFileAsync(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientFileResponse> GetThumbnailAsync(
        string urn,
        int caseId,
        string materialId,
        int documentId,
        int maxDimensionPixel,
        int pageIndex,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/thumbnails/{maxDimensionPixel}/{pageIndex}";
        return await this.SendFileAsync(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<PcdRequestCore>>> GetPcdRequestCoreAsync(
        string urn,
        int caseId,
        int pcdId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/pcds/{pcdId}/pcd-request-core";
        return await this.SendAsync<IEnumerable<PcdRequestCore>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<PcdRequestDto>> GetPcdRequestAsync(
            string urn,
            int caseId,
            int pcdId,
            CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/pcds/{pcdId}/pcd-request";
        return await this.SendAsync<PcdRequestDto>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientFileResponse> GetMaterialDocumentAsync(
            string urn,
            int caseId,
            int materialId,
            CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/materials/{materialId}/document";
        return await this.SendFileAsync(route, HttpMethod.Get, cancellationToken);
    }

    public record OcrPollResponse(string NextUrl);

    public async Task<ApiClientResponse<OcrPollResponse>> GetOcrPollAsync(
        string urn,
        int caseId,
        string materialId,
        long documentId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/ocr";
        return await this.SendAsync<OcrPollResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<AnalyzeResults>> GetOcrAsync(
        string urn,
        int caseId,
        string materialId,
        long documentId,
        CancellationToken cancellationToken,
        bool? isOcrProcessed = null,
        bool? forceRefresh = null,
        Guid? token = null)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/ocr";

        // optional query string support (matches function query params)
        var qs = new List<string>();
        if (isOcrProcessed.HasValue)
        {
            qs.Add($"isOcrProcessed={isOcrProcessed.Value.ToString().ToLowerInvariant()}");
        }

        if (forceRefresh.HasValue)
        {
            qs.Add($"ForceRefresh={forceRefresh.Value.ToString().ToLowerInvariant()}");
        }

        if (token.HasValue)
        {
            qs.Add($"token={token.Value}");
        }

        if (qs.Count > 0)
        {
            route = $"{route}?{string.Join("&", qs)}";
        }

        return await this.SendAsync<AnalyzeResults>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<WitnessStatementsResponse>> GetCaseWitnessStatementsHkAsync(
            string urn,
            int caseId,
            long witnessId,
            CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/witnesses/{witnessId}/witness-statements";
        return await this.SendAsync<WitnessStatementsResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientFileResponse> GetPdfAsync(
        string urn,
        int caseId,
        string materialId,
        int documentId,
        CancellationToken cancellationToken,
        bool? isOcrProcessed = null,
        bool? forceRefresh = null)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/versions/{documentId}/pdf";
        return await this.SendFileAsync(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<DocumentDto>>> GetDocumentListAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents";
        return await this.SendAsync<IEnumerable<DocumentDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<DocumentNoteDto>>> GetDocumentNotesAsync(string urn, int caseId, string materialId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/notes";
        return await this.SendAsync<IEnumerable<DocumentNoteDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<ExhibitProducerDto>>> GetExhibitProducersAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/exhibit-producers";
        return await this.SendAsync<IEnumerable<ExhibitProducerDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<MaterialTypeDto>>> GetMaterialTypeListAsync(CancellationToken cancellationToken)
    {
        var route = "reference/reclassification";
        return await this.SendAsync<IEnumerable<MaterialTypeDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<CaseWitnessDto>>> GetWitnessesAsync(string urn, int caseId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/witnesses";
        return await this.SendAsync<IEnumerable<CaseWitnessDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<WitnessStatementDto>>> GetWitnessStatementsAsync(string urn, int caseId, int witnessId, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/witnesses/{witnessId}/statements";
        return await this.SendAsync<IEnumerable<WitnessStatementDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<CaseIdentifiersDto>> LookupUrnAsync(int caseId, CancellationToken cancellationToken)
    {
        var route = $"urn-lookup/{caseId}";
        return await this.SendAsync<CaseIdentifiersDto>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<DocumentReclassifiedResultDto>> ReclassifyDocumentAsync(int urn, int caseId, string materialId, ReclassifyDocumentDto request, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/reclassify";
        return await this.SendAsync<ReclassifyDocumentDto, DocumentReclassifiedResultDto>(route, HttpMethod.Post, request, cancellationToken);
    }

    public async Task<ApiClientResponse<PcdReviewDetailResponse>> GetPcdReviewDetailsAsync(
        string urn,
        int caseId,
        int historyId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/history/{historyId}/pcd-review-details";
        return await this.SendAsync<PcdReviewDetailResponse>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse<IEnumerable<PcdReviewCoreResponseDto>>> GetPcdReviewCoreAsync(
        string urn,
        int caseId,
        CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/pcd-review-core";
        return await this.SendAsync<IEnumerable<PcdReviewCoreResponseDto>>(route, HttpMethod.Get, cancellationToken);
    }

    public async Task<ApiClientResponse> RenameDocumentAsync(string urn, int caseId, string materialId, RenameDocumentRequestDto request, CancellationToken cancellationToken)
    {
        var route = $"urns/{urn}/cases/{caseId}/documents/{materialId}/rename";
        return await this.SendAsync<RenameDocumentRequestDto>(route, HttpMethod.Put, request, cancellationToken);
    }

    private async Task<ApiClientResponse> SendAsync(string route, HttpMethod httpMethod, CancellationToken cancellationToken = default)
    {
        var token = await this.tokenAuthApiClient.GetTokenAsync(cancellationToken);
        var cmsAuthValues = await this.cmsAuthApiClient.GetCmsAuthTokenAsync(cancellationToken);
        var httpRequestMessage = this.CreateHttpRequestMessage(route, httpMethod, null, string.Empty, token, cmsAuthValues);
        var httpResponseMessage = await this.SendAsync(httpRequestMessage, cancellationToken);
        return new ApiClientResponse(httpResponseMessage);
    }

    private async Task<ApiClientResponse> SendAsync<TRequest>(string route, HttpMethod httpMethod, TRequest request, CancellationToken cancellationToken = default)
    {
        var token = await this.tokenAuthApiClient.GetTokenAsync(cancellationToken);
        var cmsAuthValues = await this.cmsAuthApiClient.GetCmsAuthTokenAsync(cancellationToken);
        var content = new StringContent(JsonSerializer.Serialize(request));
        var httpRequestMessage = this.CreateHttpRequestMessage(route, httpMethod, content, string.Empty, token, cmsAuthValues);
        var httpResponseMessage = await this.SendAsync(httpRequestMessage, cancellationToken);
        return new ApiClientResponse(httpResponseMessage);
    }

    private async Task<ApiClientResponse<TResponse>> SendAsync<TResponse>(string route, HttpMethod httpMethod, CancellationToken cancellationToken = default)
    {
        var token = await this.tokenAuthApiClient.GetTokenAsync(cancellationToken);
        var cmsAuthValues = await this.cmsAuthApiClient.GetCmsAuthTokenAsync(cancellationToken);
        var httpRequestMessage = this.CreateHttpRequestMessage(route, httpMethod, null, string.Empty, token, cmsAuthValues);
        var httpResponseMessage = await this.SendAsync(httpRequestMessage, cancellationToken);
        return new ApiClientResponse<TResponse>(httpResponseMessage);
    }

    private async Task<ApiClientResponse<TResponse>> SendAsync<TRequest, TResponse>(string route, HttpMethod httpMethod, TRequest request, CancellationToken cancellationToken = default)
    {
        var token = await this.tokenAuthApiClient.GetTokenAsync(cancellationToken);
        var cmsAuthValues = await this.cmsAuthApiClient.GetCmsAuthTokenAsync(cancellationToken);
        var content = new StringContent(JsonSerializer.Serialize(request));
        var httpRequestMessage = this.CreateHttpRequestMessage(route, httpMethod, content, string.Empty, token, cmsAuthValues);
        var httpResponseMessage = await this.SendAsync(httpRequestMessage, cancellationToken);
        return new ApiClientResponse<TResponse>(httpResponseMessage);
    }

    private async Task<ApiClientFileResponse> SendFileAsync(
        string route,
        HttpMethod httpMethod,
        CancellationToken cancellationToken = default)
    {
        var token = await this.tokenAuthApiClient.GetTokenAsync(cancellationToken);
        var cmsAuthValues = await this.cmsAuthApiClient.GetCmsAuthTokenAsync(cancellationToken);

        var httpRequestMessage = this.CreateHttpRequestMessage(route, httpMethod, null, string.Empty, token, cmsAuthValues);
        var httpResponseMessage = await this.SendAsync(httpRequestMessage, cancellationToken);

        var bytes = await httpResponseMessage.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = httpResponseMessage.Content.Headers.ContentType?.MediaType;
        var fileName = httpResponseMessage.Content.Headers.ContentDisposition?.FileName?.Trim('"');

        return new ApiClientFileResponse(httpResponseMessage.StatusCode, bytes, contentType, fileName);
    }
}
