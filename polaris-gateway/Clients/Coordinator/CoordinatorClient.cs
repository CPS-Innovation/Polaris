// <copyright file="CoordinatorClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Clients.Coordinator;

using Common.Configuration;
using Common.Constants;
using Common.Dto.Request;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class CoordinatorClient(IRequestFactory requestFactory, HttpClient httpClient)
    : ICoordinatorClient
{
    public async Task<HttpResponseMessage> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId) =>
        await this.SendRequestAsync(
            HttpMethod.Post,
            RestApi.GetCasePath(caseUrn, caseId),
            correlationId,
            cmsAuthValues);

    public async Task<HttpResponseMessage> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId) =>
        await this.SendRequestAsync(
            HttpMethod.Delete,
            RestApi.GetCasePath(caseUrn, caseId),
            correlationId,
            cmsAuthValues);

    public async Task<HttpResponseMessage> GetTrackerGetCaseAsync(string caseUrn, int caseId, Guid correlationId)
    {
        var response = await this.SendRequestAsync(
            HttpMethod.Get,
            RestApi.GetCaseTrackerPath(caseUrn, caseId),
            correlationId,
            skipRetry: true);

        // #27357 we return 404 if 503 or 502 status code is returned. The client handles 404s and continues to poll
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable || response.StatusCode == HttpStatusCode.BadGateway)
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        return response;
    }

    public async Task<HttpResponseMessage> GetTrackerBulkRedactionSearchAsync(string caseUrn, int caseId, string materialId, long documentId, string searchText, Guid correlationId, CancellationToken cancellationToken = default)
    {
        var response = await this.SendRequestAsync(
            HttpMethod.Get,
            RestApi.GetBulkRedactionSearchTrackerPath(caseUrn, caseId, materialId, documentId, searchText),
            correlationId,
            skipRetry: true,
            cancellationToken: cancellationToken);

        // #27357 we return 404 if 503 or 502 status code is returned. The client handles 404s and continues to poll
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable || response.StatusCode == HttpStatusCode.BadGateway)
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        return response;
    }

    public async Task<HttpResponseMessage> SaveRedactionsAsync(string caseUrn, int caseId, string materialId, long documentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId)
    {
        return await this.SendRequestAsync(
            HttpMethod.Put,
            RestApi.GetRedactDocumentPath(caseUrn, caseId, materialId, documentId),
            correlationId,
            cmsAuthValues,
            new StringContent(JsonSerializer.Serialize(redactPdfRequest), Encoding.UTF8, ContentType.Json));
    }

    public async Task<HttpResponseMessage> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId)
    {
        return await this.SendRequestAsync(
            HttpMethod.Get,
            RestApi.GetCaseSearchQueryPath(caseUrn, caseId, searchTerm),
            correlationId);
    }

    public async Task<HttpResponseMessage> GetCaseSearchIndexCount(string caseUrn, int caseId, Guid correlationId) =>
        await this.SendRequestAsync(
            HttpMethod.Get,
            RestApi.CaseSearchCountPath(caseUrn, caseId),
            correlationId);

    public async Task<HttpResponseMessage> ModifyDocument(string caseUrn, int caseId, string materialId, long documentId, ModifyDocumentDto modifyDocumentDto, string cmsAuthValues, Guid correlationId) =>
        await this.SendRequestAsync(
            HttpMethod.Post,
            RestApi.GetModifyDocumentPath(caseUrn, caseId, materialId, documentId),
            correlationId,
            cmsAuthValues,
            new StringContent(JsonSerializer.Serialize(modifyDocumentDto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), Encoding.UTF8, ContentType.Json));

    public async Task<HttpResponseMessage> BulkRedactionInitiateSearchAsync(string caseUrn, int caseId, string materialId, long documentId,
        Guid correlationId, string cmsAuthValues, CancellationToken cancellationToken = default) =>
        await this.SendRequestAsync(
            HttpMethod.Post,
            RestApi.GetBulkRedactionSearchStartPath(caseUrn, caseId, materialId, documentId),
            correlationId,
            cmsAuthValues,
            cancellationToken: cancellationToken);

    public async Task<HttpResponseMessage> BulkRedactionRetrieveSearchResultsAsync(string caseUrn, int caseId, string materialId, long documentId, string searchText, 
        Guid correlationId, string cmsAuthValues, CancellationToken cancellationToken = default) =>
        await this.SendRequestAsync(
            HttpMethod.Get,
            RestApi.GetBulkRedactionSearchResultsPath(caseUrn, caseId, materialId, documentId, searchText),
            correlationId,
            cmsAuthValues,
            cancellationToken: cancellationToken);

    private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod httpMethod, string requestUri, Guid correlationId, string cmsAuthValues = null, HttpContent content = null, bool skipRetry = false, CancellationToken cancellationToken = default)
    {
        var request = requestFactory.Create(httpMethod, requestUri, correlationId, cmsAuthValues, content);
        if (skipRetry)
        {
            request.Headers.Add("X-Skip-Retry", "true");
        }

        return await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }
}
