// <copyright file="ApiServiceClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Configuration.Extensions;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Configuration;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Diagnostics;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;
using Microsoft;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Utils;
using Cps.Fct.Hk.Ui.Interfaces.Model.PCD;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model.Requests.PcdRequests;
using System.Net;
using System.Linq;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model.Requests;
using System.Text.Json.Serialization;

/// <summary>
/// The DDEI API service client.
/// </summary>
public class ApiServiceClient : IDdeiServiceClient
{
    private readonly ILogger<ApiServiceClient> logger;
    private readonly IHttpClientFactory clientFactory;
    private readonly ClientEndpointOptions clientOptions;
    private readonly object sync = new();
    private AuthenticationHeader? authenticationHeader;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiServiceClient"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="clientFactory">The HTTP client factory.</param>
    /// <param name="clientOptions">The client endpoint options.</param>
    public ApiServiceClient(
        ILoggerFactory loggerFactory,
        IHttpClientFactory clientFactory,
        IOptions<ClientEndpointOptions> clientOptions)
    {
        Requires.NotNull(loggerFactory);
        Requires.NotNull(clientFactory);
        Requires.NotNull(clientOptions);

        this.logger = loggerFactory.CreateLogger<ApiServiceClient>();
        this.clientFactory = clientFactory;
        this.clientOptions = clientOptions.Value;
    }

    /// <inheritdoc/>
    public async Task<string?> GetCmsModernTokenAsync(GetCmsModernTokenRequest request)
    {
        Requires.NotNull(request);
        Requires.NotNull(request.CmsAuthValues);

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "GetCmsModernToken";
        string? results = null;
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);
            path = this.clientOptions.RelativePath[OperationName];

            HttpRequestMessage httpRequest = new(method: HttpMethod.Get, requestUri: path);

            if (string.IsNullOrEmpty(request.CmsAuthValues.CmsCookies))
            {
                throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Cookies cannot be null or empty.", nameof(request));
            }

            this.EnsureAuthenticationHeaders(httpRequest, request.CmsAuthValues.CmsCookies, request.CorrespondenceId, generateTokenForCmsModernTokenRetrieval: true);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            CmsModernTokenResult? data = JsonSerializer.Deserialize<CmsModernTokenResult>(json: content, options: ApplicationOptions.ApplicationSerializerOptions);
            if (data?.CmsModernToken is not null)
            {
                results = data.CmsModernToken.ToString();
            }

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, "");
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<CaseSummary?> GetCaseSummaryAsync(GetCaseSummaryRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "GetCaseSummary";
        CaseSummary? results = null;
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture));

            HttpRequestMessage httpRequest = new(method: HttpMethod.Get, requestUri: path);

            this.EnsureAuthenticationHeaders(httpRequest, cmsAuthValues.CmsCookies, request.CorrespondenceId, cmsAuthValues.CmsModernToken);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            CaseSummary? data = JsonSerializer.Deserialize<CaseSummary>(json: content, options: ApplicationOptions.ApplicationSerializerOptions);
            if (data?.Urn is not null)
            {
                results = new CaseSummary(
                    data.CaseId,
                    data.Urn,
                    data.LeadDefendantFirstNames,
                    data.LeadDefendantSurname,
                    data.NumberOfDefendants,
                    data.UnitName);
            }

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, "");
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Communication>> ListCommunicationsHkAsync(ListCommunicationsHkRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        if (string.IsNullOrEmpty(cmsAuthValues.CmsCookies))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Cookies cannot be null or empty.", nameof(request));
        }

        if (string.IsNullOrEmpty(cmsAuthValues.CmsModernToken))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Modern Token cannot be null or empty.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "ListCommunicationsHk";
        List<Communication> results = [];
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture));

            HttpRequestMessage httpRequest = new(method: HttpMethod.Get, requestUri: path);

            this.EnsureAuthenticationHeaders(httpRequest, cmsAuthValues.CmsCookies, request.CorrespondenceId, cmsAuthValues.CmsModernToken);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            string additionalInfo = $"received #0 communications";
            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            IEnumerable<Communication>? data = JsonSerializer.Deserialize<IEnumerable<Communication>>(json: content, options: ApplicationOptions.ApplicationSerializerOptions);
            if (data is not null)
            {
                results = new(data);
                if (results.Count != 0)
                {
                    additionalInfo = $"received #{results.Count} communications";
                }
            }

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, additionalInfo);
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<UsedStatementsResponse> GetUsedStatementsAsync(GetUsedStatementsRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        if (string.IsNullOrEmpty(cmsAuthValues.CmsCookies))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Cookies cannot be null or empty.", nameof(request));
        }

        if (string.IsNullOrEmpty(cmsAuthValues.CmsModernToken))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Modern Token cannot be null or empty.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "GetUsedStatements";
        UsedStatementsResponse results = new();
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture));

            HttpRequestMessage httpRequest = new(method: HttpMethod.Get, requestUri: path);

            this.EnsureAuthenticationHeaders(httpRequest, cmsAuthValues.CmsCookies, request.CorrespondenceId, cmsAuthValues.CmsModernToken);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            string additionalInfo = $"received #0 used statements";
            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            UsedStatementsResponse? data = JsonSerializer.Deserialize<UsedStatementsResponse>(json: content, options: ApplicationOptions.ApplicationSerializerOptions);

            if (data?.Statements is not null)
            {
                results = new UsedStatementsResponse
                {
                    Statements = data.Statements.Select(statement => new Statement(
                        statement.Id,
                        statement.WitnessId,
                        statement.Title,
                        statement.OriginalFileName,
                        statement.MaterialType,
                        statement.DocumentType,
                        statement.Link,
                        statement.Status,
                        statement.ReceivedDate,
                        statement.StatementTakenDate
                    )).ToList()
                };

                if (results.Statements.Count != 0)
                {
                    additionalInfo = $"received #{results.Statements.Count} used statements";
                }

            }

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, additionalInfo);
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<UsedExhibitsResponse> GetUsedExhibitsAsync(GetUsedExhibitsRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        if (string.IsNullOrEmpty(cmsAuthValues.CmsCookies))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Cookies cannot be null or empty.", nameof(request));
        }

        if (string.IsNullOrEmpty(cmsAuthValues.CmsModernToken))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Modern Token cannot be null or empty.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "GetUsedExhibits";
        UsedExhibitsResponse results = new();
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture));

            HttpRequestMessage httpRequest = new(method: HttpMethod.Get, requestUri: path);

            this.EnsureAuthenticationHeaders(httpRequest, cmsAuthValues.CmsCookies, request.CorrespondenceId, cmsAuthValues.CmsModernToken);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            string additionalInfo = $"received #0 used exhibits";
            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            UsedExhibitsResponse? data = JsonSerializer.Deserialize<UsedExhibitsResponse>(json: content, options: ApplicationOptions.ApplicationSerializerOptions);

            if (data?.Exhibits is not null)
            {
                results = new UsedExhibitsResponse
                {
                    Exhibits = data.Exhibits.Select(exhibit => new Exhibit(
                        exhibit.Id,
                        exhibit.Title,
                        exhibit.OriginalFileName,
                        exhibit.MaterialType,
                        exhibit.DocumentType,
                        exhibit.Link,
                        exhibit.Status,
                        exhibit.ReceivedDate,
                        exhibit.Reference,
                        exhibit.Producer
                    )).ToList()
                };

                if (results.Exhibits.Count != 0)
                {
                    additionalInfo = $"received #{results.Exhibits.Count} used exhibits";
                }
            }

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, additionalInfo);
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<UsedMgFormsResponse> GetUsedMgFormsAsync(GetUsedMgFormsRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        if (string.IsNullOrEmpty(cmsAuthValues.CmsCookies))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Cookies cannot be null or empty.", nameof(request));
        }

        if (string.IsNullOrEmpty(cmsAuthValues.CmsModernToken))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Modern Token cannot be null or empty.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "GetUsedMgForms";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture));

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            UsedMgFormsResponse usedMgForms = await this.HandleHttpRequestInternalAsync<UsedMgFormsResponse>(
                httpRequest,
                cmsAuthValues,
                request,
                OperationName).ConfigureAwait(false);

            // Ensure we always return a valid response
            usedMgForms ??= new UsedMgFormsResponse { MgForms = new List<MgForm>() };

            string additionalInfo = usedMgForms?.MgForms is { Count: > 0 }
                ? $"received #{usedMgForms.MgForms.Count} used MG forms"
                : "received #0 used MG forms";

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, additionalInfo);
            return usedMgForms!;
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<UsedOtherMaterialsResponse> GetUsedOtherMaterialsAsync(GetUsedOtherMaterialsRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        if (string.IsNullOrEmpty(cmsAuthValues.CmsCookies))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Cookies cannot be null or empty.", nameof(request));
        }

        if (string.IsNullOrEmpty(cmsAuthValues.CmsModernToken))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Modern Token cannot be null or empty.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "GetUsedOtherMaterials";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture));

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            UsedOtherMaterialsResponse usedOtherMaterials = await this.HandleHttpRequestInternalAsync<UsedOtherMaterialsResponse>(
                httpRequest,
                cmsAuthValues,
                request,
                OperationName).ConfigureAwait(false);

            // Ensure we always return a valid response
            usedOtherMaterials ??= new UsedOtherMaterialsResponse { MgForms = new List<MgForm>() };

            string additionalInfo = usedOtherMaterials?.MgForms is { Count: > 0 }
                ? $"received #{usedOtherMaterials.MgForms.Count} used other materials"
                : "received #0 used other materials";

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, additionalInfo);
            return usedOtherMaterials!;
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<UnusedMaterialsResponse> GetUnusedMaterialsAsync(GetUnusedMaterialsRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        if (string.IsNullOrEmpty(cmsAuthValues.CmsCookies))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Cookies cannot be null or empty.", nameof(request));
        }

        if (string.IsNullOrEmpty(cmsAuthValues.CmsModernToken))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Modern Token cannot be null or empty.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "GetUnusedMaterials";
        UnusedMaterialsResponse results = new();
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture));

            HttpRequestMessage httpRequest = new(method: HttpMethod.Get, requestUri: path);

            this.EnsureAuthenticationHeaders(httpRequest, cmsAuthValues.CmsCookies, request.CorrespondenceId, cmsAuthValues.CmsModernToken);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            string additionalInfo = $"received #0 unused materials";
            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            UnusedMaterialsResponse? data = JsonSerializer.Deserialize<UnusedMaterialsResponse>(json: content, options: ApplicationOptions.ApplicationSerializerOptions);

            if (data is not null)
            {
                results = new UnusedMaterialsResponse
                {
                    Exhibits = data.Exhibits?.Select(exhibit => new Exhibit(
                        exhibit.Id,
                        exhibit.Title,
                        exhibit.OriginalFileName,
                        exhibit.MaterialType,
                        exhibit.DocumentType,
                        exhibit.Link,
                        exhibit.Status,
                        exhibit.ReceivedDate,
                        exhibit.Reference,
                        exhibit.Producer
                    )).ToList(),

                    MgForms = data.MgForms?.Select(mgForm => new MgForm(
                        mgForm.Id,
                        mgForm.Title,
                        mgForm.OriginalFileName,
                        mgForm.MaterialType,
                        mgForm.DocumentType,
                        mgForm.Link,
                        mgForm.Status,
                        mgForm.Date
                    )).ToList(),

                    OtherMaterials = data.OtherMaterials?.Select(otherMaterial => new MgForm(
                        otherMaterial.Id,
                        otherMaterial.Title,
                        otherMaterial.OriginalFileName,
                        otherMaterial.MaterialType,
                        otherMaterial.DocumentType,
                        otherMaterial.Link,
                        otherMaterial.Status,
                        otherMaterial.Date
                    )).ToList(),

                    Statements = data.Statements?.Select(statement => new Statement(
                        statement.Id,
                        statement.WitnessId,
                        statement.Title,
                        statement.OriginalFileName,
                        statement.MaterialType,
                        statement.DocumentType,
                        statement.Link,
                        statement.Status,
                        statement.ReceivedDate,
                        statement.StatementTakenDate
                    )).ToList()
                };

                // Generate additional info string based on the collections
                additionalInfo = this.BuildAdditionalInfo(results);
            }

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, additionalInfo);
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<FileStreamResult?> GetMaterialDocumentAsync(GetDocumentRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "GetMaterialDocument";
        FileStreamResult? results;
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture))
                .Replace("{*filePath}", request.FilePath.ToString(CultureInfo.InvariantCulture));

            HttpRequestMessage httpRequest = new(method: HttpMethod.Get, requestUri: path);

            this.EnsureAuthenticationHeaders(httpRequest, cmsAuthValues.CmsCookies, request.CorrespondenceId, cmsAuthValues.CmsModernToken);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            // Read the response content into a MemoryStream
            var memoryStream = new MemoryStream();
            await httpResponse.Content.CopyToAsync(memoryStream).ConfigureAwait(false);

            // Reset the position of the MemoryStream to the beginning
            memoryStream.Position = 0;

            string sanitizedFilePath = HttpUtility.UrlDecode(path);
            string fileDownloadName = Path.GetFileName(sanitizedFilePath);
            string? contentType = FileUtils.GetMimeType(fileDownloadName) ??
                throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} Content type cannot be determined for file: {fileDownloadName}");

            // Create FileStreamResult using the stream and content type
            results = new FileStreamResult(memoryStream, contentType)
            {
                FileDownloadName = fileDownloadName,
            };

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, fileDownloadName);
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<AttachmentsResponse> GetAttachmentsAsync(GetAttachmentsRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        if (string.IsNullOrEmpty(cmsAuthValues.CmsCookies))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Cookies cannot be null or empty.", nameof(request));
        }

        if (string.IsNullOrEmpty(cmsAuthValues.CmsModernToken))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Modern Token cannot be null or empty.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "GetAttachments";
        AttachmentsResponse results = new();
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{communicationId}", request.CommunicationId.ToString(CultureInfo.InvariantCulture));

            HttpRequestMessage httpRequest = new(method: HttpMethod.Get, requestUri: path);

            this.EnsureAuthenticationHeaders(httpRequest, cmsAuthValues.CmsCookies, request.CorrespondenceId, cmsAuthValues.CmsModernToken);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            string additionalInfo = $"received #0 attachments";
            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            AttachmentsResponse? data = JsonSerializer.Deserialize<AttachmentsResponse>(json: content, options: ApplicationOptions.ApplicationSerializerOptions);

            if (data?.Attachments is not null)
            {
                results = new AttachmentsResponse
                {
                    Attachments = data.Attachments.Select(attachment => new Interfaces.Model.Attachment(
                        attachment.MaterialId,
                        attachment.Name,
                        attachment.Description,
                        attachment.Link,
                        attachment.Classification,
                        attachment.DocumentTypeId,
                        attachment.NumOfDocVersions,
                        attachment.Statement,
                        attachment.Exhibit,
                        attachment.Tag,
                        attachment.DocId,
                        attachment.OriginalFileName,
                        attachment.CheckedOutTo,
                        attachment.DocumentId,
                        attachment.OcrProcessed,
                        attachment.Direction
                    )).ToList()
                };

                if (results.Attachments.Count != 0)
                {
                    additionalInfo = $"received #{results.Attachments.Count} attachments";
                }
            }

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, additionalInfo);
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<ReclassificationResponse> ReclassifyCommunicationAsync(ReclassifyCommunicationRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "ReclassifyCommunication";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName];

            Guid correspondenceId = request.CorrespondenceId != Guid.Empty ? request.CorrespondenceId : Guid.NewGuid();

            // Serialize the request object to JSON
            string jsonRequest = JsonSerializer.Serialize(new
            {
                request.classification,
                request.materialId,
                request.documentTypeId,
                request.used,
                request.subject,
                request.Statement,
                request.Exhibit
            });

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            };

            this.EnsureAuthenticationHeaders(httpRequest, cmsAuthValues.CmsCookies, correspondenceId, cmsAuthValues.CmsModernToken);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            ReclassificationResponse? reclassificationResponse = null;
            try
            {
                reclassificationResponse = JsonSerializer.Deserialize<ReclassificationResponse>(content, ApplicationOptions.ApplicationSerializerOptions);

                if (reclassificationResponse == null || reclassificationResponse.ReclassifyCommunication == null)
                {
                    throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} Failed to deserialize ReclassificationResponse. JSON content: {content}");
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} Failed to deserialize ReclassificationResponse due to JSON exception.", ex);
            }

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, $"materialId [{request.materialId}]");
            return reclassificationResponse;
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    /// <summary>
    /// Builds a string containing information about the unused materials, based on the provided response.
    /// The information includes the number of unused exhibits, mg forms, other materials, and statements.
    /// </summary>
    /// <param name="results">The response object containing the lists of unused materials.</param>
    /// <returns>A string that describes the received unused materials, including counts for each type.</returns>
    public string BuildAdditionalInfo(UnusedMaterialsResponse results)
    {
        string additionalInfo = string.Empty;
        bool addedInfo = false;

        if (results.Exhibits?.Count > 0)
        {
            additionalInfo += $"received #{results.Exhibits.Count} unused exhibits";
            addedInfo = true;
        }

        if (results.MgForms?.Count > 0)
        {
            additionalInfo += (addedInfo ? "," : "") + $" received #{results.MgForms.Count} unused mg forms";
            addedInfo = true;
        }

        if (results.OtherMaterials?.Count > 0)
        {
            additionalInfo += (addedInfo ? "," : "") + $" received #{results.OtherMaterials.Count} unused other materials";
            addedInfo = true;
        }

        if (results.Statements?.Count > 0)
        {
            additionalInfo += (addedInfo ? "," : "") + $" received #{results.Statements.Count} unused statements";
        }

        if (!addedInfo)
        {
            additionalInfo = "received #0 unused materials";
        }

        return additionalInfo;
    }

    /// <summary>
    /// Handles an exception that occurred while calling the DDEI API.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="path">The relative path of the API call.</param>
    /// <param name="errorResponse">An error response from DDEI API if one is received.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="urn">The URN.</param>
    /// <param name="request">The request with a correspondence ID.</param>
    /// <param name="duration">The duration of the operation.</param>
    public void HandleException(
        string operationName,
        string path,
        string? errorResponse,
        Exception exception,
        BaseRequest request,
        TimeSpan duration)
    {
        Requires.NotNull(operationName);
        Requires.NotNull(path);
        Requires.NotNull(exception);

        const string LogMessage = DiagnosticsUtility.Error + @"Calling the DDEI-EAS API failed for {Operation} after {Duration}. Path: {Path}, Correspondence ID: {CorrespondenceId}, Failure: {Reason}
 - Failure response: {FailureResponse}";
        this.logger.LogError(
            exception,
            LoggingConstants.HskUiLogPrefix + " " + LogMessage,
            operationName,
            duration,
            path,
            request?.CorrespondenceId,
            exception.ToAggregatedMessage(),
            errorResponse);
    }

    /// <summary>
    /// Logs an operation completed event.
    /// </summary>
    /// <param name="operationName">The operation name.</param>
    /// <param name="path">The relative path of the API call.</param>
    /// <param name="urn">The URN.</param>
    /// <param name="request">The request with a correspondence ID.</param>
    /// <param name="duration">The duration of the operation.</param>
    /// <param name="additionalInfo">Any additional information.</param>
    public void LogOperationCompletedEvent(
        string operationName,
        string path,
        BaseRequest request,
        TimeSpan duration,
        string additionalInfo)
    {
        const string LogMessage = @"Calling the DDEI-EAS API succeeded for {Operation} after {Duration}. Path: {Path}, Correspondence ID: {CorrespondenceId}
 - Additional info: {AdditionalInfo}";

        this.logger.LogInformation(
            LoggingConstants.HskUiLogPrefix + " " + LogMessage,
            operationName,
            duration,
            path,
            request.CorrespondenceId,
            additionalInfo);
    }

    /// <summary>
    /// Asynchronously determines the error response from a failed HTTP request.
    /// </summary>
    /// <param name="httpResponse">The <see cref="HttpResponseMessage"/> received from an HTTP request.</param>
    /// <returns>
    /// A string containing the error response content if the request was not successful; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method checks if the HTTP response indicates a failure. If the status code indicates an error, the response content is read and returned as a string.
    /// Otherwise, the method returns <c>null</c>.
    /// </remarks>
    private static async Task<string?> DetermineDdeiErrorResponse(HttpResponseMessage httpResponse)
    {
        string? errorResponse = null;
        if (!httpResponse.IsSuccessStatusCode)
        {
            errorResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        return errorResponse;
    }

    /// <summary>
    /// Sets the function key header on the specified HTTP request.
    /// </summary>
    /// <param name="httpRequest">The <see cref="HttpRequestMessage"/> to which the function key header will be added.</param>
    /// <remarks>
    /// This method adds the `x-functions-key` header to the provided HTTP request using the function key stored in the client options. 
    /// If the function key is not null or whitespace and is longer than seven characters, a sanitised version of the key (showing only the last seven characters) is logged for security purposes.
    /// </remarks>
    private void SetFunctionKeyHeader(HttpRequestMessage httpRequest)
    {
        const int Seven = 7;
        string sanitisedFunctionKey = "Unknown";
        if (!string.IsNullOrWhiteSpace(this.clientOptions.FunctionKey))
        {
            httpRequest.Headers.Add("x-functions-key", this.clientOptions.FunctionKey);
            if (this.clientOptions.FunctionKey.Length > Seven)
            {
                // Last 7 characters
                sanitisedFunctionKey = $"...{this.clientOptions.FunctionKey[^Seven..]}";
            }
        }

        this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Calling the DDEI-EAS API with sanitised function key '{sanitisedFunctionKey}'");
    }

    /// <summary>
    /// Ensures that the necessary authentication headers are added to the specified HTTP request message.
    /// </summary>
    /// <param name="httpRequest">The HTTP request message to which the authentication headers will be added.</param>
    /// <param name="cmsCookies">The CMS cookies used for authentication.</param>
    /// <param name="correspondenceId">The unique identifier for the correspondence associated with the request.</param>
    /// <param name="cmsToken">An optional CMS token used for authentication. If not provided, a new token will be generated if specified.</param>
    /// <param name="generateTokenForCmsModernTokenRetrieval">Indicates whether to generate a new token for CMS Modern token retrieval. Defaults to false.</param>
    public virtual void EnsureAuthenticationHeaders(
        HttpRequestMessage httpRequest,
        string cmsCookies,
        Guid correspondenceId,
        string? cmsToken = null,
        bool generateTokenForCmsModernTokenRetrieval = false)
    {
        if (string.IsNullOrWhiteSpace(cmsCookies))
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} CmsCookies is null or empty");
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CmsCookies is null or empty");
            //return;
        }

        // The special characters encoded as follows:
        // = becomes %3D
        // ; becomes %3B
        // Spaces become %20

        string? encodedCc = cmsCookies;
        string? encodedCt = generateTokenForCmsModernTokenRetrieval ? Guid.NewGuid().ToString() : cmsToken;

        try
        {
            string decodedCc = Uri.UnescapeDataString(encodedCc);
            string decodedCt = Uri.UnescapeDataString(encodedCt ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(decodedCc) && decodedCt != null)
            {
                lock (this.sync)
                {
                    if (this.authenticationHeader is not null && this.authenticationHeader.ExpiryTime < DateTimeOffset.UtcNow.AddHours(1))
                    {
                        this.authenticationHeader = null;
                    }
                    if (this.authenticationHeader is null)
                    {
                        var authenticationContext = new AuthenticationContext(decodedCc, decodedCt, DateTimeOffset.UtcNow.AddHours(1));
                        this.authenticationHeader = new AuthenticationHeader(authenticationContext);
                    }
                }
            }

            if (this.authenticationHeader is not null)
            {
                httpRequest.Headers.Add(AuthenticationHeader.AuthenticationHeaderName, this.authenticationHeader.ToString());
            }

            this.SetFunctionKeyHeader(httpRequest);
            httpRequest.Headers.Add("Correlation-Id", $"{correspondenceId}");
        }
        catch (UriFormatException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Failed to decode URI components");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} An unexpected error occurred while setting authentication headers");
        }
    }

    /// <inheritdoc/>
    public async Task<RenameMaterialResponse> RenameMaterialAsync(RenameMaterialRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(request.materialId);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "RenameMaterial";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{materialId}", request.materialId.ToString(CultureInfo.InvariantCulture));

            Guid correspondenceId = request.CorrespondenceId != Guid.Empty ? request.CorrespondenceId : default;

            // Serialize the request object to JSON
            string jsonRequest = JsonSerializer.Serialize(new
            {
                request.materialId,
                request.subject
            });

            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, path)
            {
                Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            };

            this.EnsureAuthenticationHeaders(httpRequest, cmsAuthValues.CmsCookies, correspondenceId, cmsAuthValues.CmsModernToken);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            RenameMaterialResponse? renameMaterialResponse = null;
            try
            {
                renameMaterialResponse = JsonSerializer.Deserialize<RenameMaterialResponse>(content, ApplicationOptions.ApplicationSerializerOptions);

                if (renameMaterialResponse == null || renameMaterialResponse.RenameMaterialData == null)
                {
                    throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} Failed to deserialize RenameMaterialResponse. JSON content: {content}");
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} Failed to deserialize RenameMaterialResponse due to JSON exception.", ex);
            }

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, $"materialId [{request.materialId}]");
            return renameMaterialResponse;
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<PcdRequestCore>> GetPcdRequestCoreAsync(GetPcdRequestsCoreRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        if (string.IsNullOrEmpty(cmsAuthValues.CmsCookies))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Cookies cannot be null or empty.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "PcdRequestCore";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.caseId.ToString(CultureInfo.InvariantCulture));

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            List<PcdRequestCore>? pcdRequestCore = await this.HandleHttpRequestInternalAsync<List<PcdRequestCore>>(
                httpRequest,
                cmsAuthValues,
                request,
                OperationName).ConfigureAwait(false);

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, $"caseId [{request.caseId}]");
            return [.. pcdRequestCore];
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<PcdRequestDto>> GetPcdRequestOverviewAsync(GetPcdRequestsOverviewRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        if (string.IsNullOrEmpty(cmsAuthValues.CmsCookies))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Cookies cannot be null or empty.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "PcdRequestOverview";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.caseId.ToString(CultureInfo.InvariantCulture));

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            List<PcdRequestDto>? pcdRequests = await this.HandleHttpRequestInternalAsync<List<PcdRequestDto>>(
                httpRequest,
                cmsAuthValues,
                request,
                OperationName).ConfigureAwait(false);

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, $"caseId [{request.caseId}]");
            return [.. pcdRequests];
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<PcdRequestDto> GetPcdRequestByPcdIdAsync(GetPcdRequestByPcdIdCoreRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        if (string.IsNullOrEmpty(cmsAuthValues.CmsCookies))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Cookies cannot be null or empty.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "PcdRequestById";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.caseId.ToString(CultureInfo.InvariantCulture))
                .Replace("{pcdId}", request.pcdId.ToString(CultureInfo.InvariantCulture));

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            PcdRequestDto? pcdRequestInfo = await this.HandleHttpRequestInternalAsync<PcdRequestDto>(
                httpRequest,
                cmsAuthValues,
                request,
                OperationName).ConfigureAwait(false);

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, $"caseId [{request.caseId}]");
            return pcdRequestInfo;
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    /// <inheritdoc cref="IDdeiServiceClient.SetMaterialReadStatusAsync(SetMaterialReadStatusRequest, CmsAuthValues)"/>
    public async Task<SetMaterialReadStatusResponse> SetMaterialReadStatusAsync(SetMaterialReadStatusRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        if (string.IsNullOrEmpty(cmsAuthValues.CmsCookies))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} CMS Cookies cannot be null or empty.", nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "SetMaterialReadStatus";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{materialId}", request.materialId.ToString(CultureInfo.InvariantCulture));

            string jsonRequest = JsonSerializer.Serialize(request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, path)
            {
                Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            };

            SetMaterialReadStatusResponse? setMaterialReadStatusResponse = await this.HandleHttpRequestInternalAsync<SetMaterialReadStatusResponse>(
                httpRequest,
                cmsAuthValues,
                request,
                OperationName).ConfigureAwait(false);

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, $"materialId [{request.materialId}]");
            return setMaterialReadStatusResponse;
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<DiscardMaterialResponse> DiscardMaterialAsync(DiscardMaterialRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(request.materialId);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "DiscardMaterial";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{materialId}", request.materialId.ToString(CultureInfo.InvariantCulture));

            // Serialize the request object to JSON
            string jsonRequest = JsonSerializer.Serialize(new
            {
                request.materialId,
                request.discardReason,
                request.discardReasonDescription,
            });

            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, path)
            {
                Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            };

            DiscardMaterialResponse? discardMaterialResponse = await this.HandleHttpRequestInternalAsync<DiscardMaterialResponse>(
                httpRequest,
                cmsAuthValues,
                request,
                OperationName).ConfigureAwait(false);

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, $"materialId [{request.materialId}]");
            return discardMaterialResponse;
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<WitnessesResponse> GetWitnessesForCaseAsync(GetWitnessesForCaseRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "ListCaseWitnesses";
        WitnessesResponse results = new();
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture));

            HttpRequestMessage httpRequest = new(method: HttpMethod.Get, requestUri: path);

            this.EnsureAuthenticationHeaders(httpRequest, cmsAuthValues.CmsCookies, request.CorrespondenceId, cmsAuthValues.CmsModernToken);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            string additionalInfo = $"received #0 witnesses";
            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            List<Witness>? witnessesData = JsonSerializer.Deserialize<List<Witness>>(json: content, options: ApplicationOptions.ApplicationSerializerOptions);
            if (witnessesData is not null)
            {
                results = new WitnessesResponse
                {
                    Witnesses = witnessesData.Select(witness => new Witness(
                        witness.CaseId,
                        witness.WitnessId,
                        witness.FirstName,
                        witness.Surname)).ToList(),
                };

                if (results.Witnesses.Count > 0)
                {
                    additionalInfo = $"retrieved #{results.Witnesses.Count} witnesses";
                }
            }

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, additionalInfo);
        }

        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<StatementsForWitnessResponse> GetStatementsForWitnessAsync(GetStatementsForWitnessRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "GetStatementsForWitness";
        StatementsForWitnessResponse results = new();
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{witnessId}", request.WitnessId.ToString(CultureInfo.InvariantCulture));

            HttpRequestMessage httpRequest = new(method: HttpMethod.Get, requestUri: path);

            this.EnsureAuthenticationHeaders(httpRequest, cmsAuthValues.CmsCookies, request.CorrespondenceId, cmsAuthValues.CmsModernToken);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            string additionalInfo = "received #0 statements";
            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            StatementsForWitnessResponse? statementsData = JsonSerializer.Deserialize<StatementsForWitnessResponse>(json: content, options: ApplicationOptions.ApplicationSerializerOptions);

            if (statementsData != null)
            {
                results = new StatementsForWitnessResponse
                {
                    StatementsForWitness = statementsData.StatementsForWitness?.Select(x => new StatementForWitness(
                        x.Id,
                        x.StatementNumber)).ToList(),
                };

                if (results.StatementsForWitness?.Count > 0)
                {
                    additionalInfo = $"retrieved #{results.StatementsForWitness.Count} statements";
                }
            }

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, additionalInfo);
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<DefendantsResponse?> ListCaseDefendantsAsync(ListCaseDefendantsRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "ListCaseDefendants";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture));

            HttpRequestMessage httpRequest = new(method: HttpMethod.Get, requestUri: path);

            List<Defendant>? defendants = await this.HandleHttpRequestInternalAsync<List<Defendant>>(
                httpRequest,
                cmsAuthValues,
                request,
                OperationName).ConfigureAwait(false);

            var listCaseDefendantsResponse = new DefendantsResponse
            {
                Defendants = defendants ?? []
            };

            string additionalInfo = listCaseDefendantsResponse?.Defendants is { Count: > 0 }
                ? $"received #{listCaseDefendantsResponse.Defendants.Count} defendants"
                : "received #0 defendants";

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, additionalInfo);
            return listCaseDefendantsResponse;
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    public async Task<ExhibitProducersResponse> GetExhibitProducersAsync(GetExhibitProducersRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "GetExhibitProducers";
        ExhibitProducersResponse results = new();
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture));

            HttpRequestMessage httpRequest = new(method: HttpMethod.Get, requestUri: path);

            this.EnsureAuthenticationHeaders(httpRequest, cmsAuthValues.CmsCookies, request.CorrespondenceId, cmsAuthValues.CmsModernToken);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);
            httpResponse.EnsureSuccessStatusCode();

            string additionalInfo = "received #0 producers";
            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            ExhibitProducersResponse? producersData = JsonSerializer.Deserialize<ExhibitProducersResponse>(json: content, options: ApplicationOptions.ApplicationSerializerOptions);

            if (producersData != null)
            {
                results = new ExhibitProducersResponse
                {
                    ExhibitProducers = producersData.ExhibitProducers?.Select(x => new ExhibitProducer(
                        x.Id,
                        x.Name)).ToList(),
                };

                if (results.ExhibitProducers?.Count > 0)
                {
                    additionalInfo = $"retrieved #{results.ExhibitProducers.Count} producers";
                }
            }

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, additionalInfo);
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }

        return results;
    }

    public async Task<NoContentResult> AddWitnessAsync(AddWitnessRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(request.caseId);
        Requires.NotNull(request.Surname);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "AddWitness";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.caseId.ToString(CultureInfo.InvariantCulture));

            string jsonRequest = JsonSerializer.Serialize(new
            {
                request.caseId,
                request.FirstName,
                request.Surname
            });

            HttpRequestMessage httpRequest = new(method: HttpMethod.Post, requestUri: path)
            {
                Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            };

            NoContentResult? response = await this.HandleHttpRequestInternalAsync<NoContentResult>(
                httpRequest,
                cmsAuthValues,
                request,
                OperationName).ConfigureAwait(false);

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, string.Empty);
            return response;
        }

        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<NoContentResult> AddCaseActionPlanAsync(int caseId, AddActionPlanRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "AddActionPlan";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            if (caseId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(caseId), "Case ID must be a positive integer.");
            }

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", caseId.ToString(CultureInfo.InvariantCulture));

            Model.Step[] mappedSteps = request.steps
                .Select(s => new Model.Step(
                    code: s.code,
                    description: s.description,
                    text: s.text,
                    hidden: s.hidden,
                    hiddenDraft: s.hiddenDraft))
                .ToArray();

            var ddeiRequest = new AddActionPlanRequest(
                Id: request.Id,
                fullDefendantName: request.fullDefendantName,
                allDefendants: request.allDefendants,
                date: request.date,
                dateExpected: request.dateExpected,
                dateTimeCreated: request.dateTimeCreated,
                type: request.type,
                actionPointText: request.actionPointText,
                status: request.status,
                statusDescription: request.statusDescription,
                dG6Justification: request.dG6Justification,
                createdByOrganisation: request.createdByOrganisation,
                expectedDateUpdated: request.expectedDateUpdated,
                partyType: request.partyType,
                policeChangeReason: request.policeChangeReason,
                statusUpdated: request.statusUpdated,
                syncedWithPolice: request.syncedWithPolice,
                cpsChangeReason: request.cpsChangeReason,
                duplicateOriginalMaterial: request.duplicateOriginalMaterial,
                material: request.material,
                chaserTaskDate: request.chaserTaskDate,
                defendantId: request.defendantId,
                steps: mappedSteps
            );

            string jsonRequest = JsonSerializer.Serialize(ddeiRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            });

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            };

            NoContentResult? addCaseActionPlanResponse = await this.HandleHttpRequestInternalAsync<NoContentResult>(
                httpRequest,
                cmsAuthValues,
                ddeiRequest,
                OperationName).ConfigureAwait(false);

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, "");
            return addCaseActionPlanResponse;
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CaseLockedStatusResult> CheckCaseLockAsync(int caseId, CmsAuthValues cmsAuthValues)
    {
        if (caseId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(caseId), "Case ID must be a positive integer.");
        }

        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "CheckCaseLock";
        string path = "Undefined";
        string? errorResponse = null;

        var dummyRequest = new BaseRequest(Guid.NewGuid());

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", caseId.ToString(CultureInfo.InvariantCulture));

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            var checkCaseLockResponse = await this.HandleHttpRequestInternalAsync<CaseLockedStatusResult>(
                httpRequest,
                cmsAuthValues,
                dummyRequest,
                OperationName).ConfigureAwait(false);

            this.LogOperationCompletedEvent(OperationName, path, dummyRequest, stopwatch.Elapsed, $"Case ID [{caseId}]");
            return checkCaseLockResponse;
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, dummyRequest, stopwatch.Elapsed);
            throw;
        }
    }

    /// <summary>
    /// Internal method to process DDEI calls to reduce code duplication.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="httpRequest"></param>
    /// <param name="cmsAuthValues"></param>
    /// <param name="request"></param>
    /// <param name="operationName"></ param >
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    /// <exception cref="HttpRequestException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<TResponse> HandleHttpRequestInternalAsync<TResponse>(HttpRequestMessage httpRequest, CmsAuthValues cmsAuthValues, BaseRequest request, string operationName)
    {
        Guid correspondenceId = request.CorrespondenceId != Guid.Empty ? request.CorrespondenceId : default;
        HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

        this.EnsureAuthenticationHeaders(
            httpRequest,
            cmsAuthValues.CmsCookies,
            correspondenceId,
            cmsAuthValues.CmsModernToken);

        HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
        string? errorResponse = await DetermineDdeiErrorResponse(httpResponse).ConfigureAwait(false);

        if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(errorResponse);
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new HttpRequestException(errorResponse);
        }

        httpResponse.EnsureSuccessStatusCode();
        string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (content.Contains("Invalid Cms Auth Values. It may be that you do not have CMS cookies", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(errorResponse);
        }

        // Check for NoContent response
        if (httpResponse.StatusCode == HttpStatusCode.NoContent)
        {
            if (typeof(TResponse) == typeof(NoContentResult))
            {
                object result = new NoContentResult();
                return (TResponse)result;
            }

            return default!;
        }

        try
        {
            TResponse? response = JsonSerializer.Deserialize<TResponse>(content, ApplicationOptions.ApplicationSerializerOptions);

            return response == null
                ? throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} Failed to deserialize response for {operationName}. JSON content: {content}")
                : response;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} Failed to deserialize response for {operationName} due to JSON exception.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<AuthenticationContext> AuthenticateAsync(AuthenticationRequest request)
    {
        Requires.NotNull(request);

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "Authenticate";
        AuthenticationContext result;
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            Dictionary<string, string> formData = new()
            {
                { "username", request.Username },
                { "password", request.Password },
            };

            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);
            path = this.clientOptions.RelativePath[OperationName];
            HttpRequestMessage httpRequest = new(method: HttpMethod.Post, requestUri: path)
            {
                Content = new FormUrlEncodedContent(formData),
            };

            this.SetFunctionKeyHeader(httpRequest);

            HttpResponseMessage httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            if (!httpResponse.IsSuccessStatusCode)
            {
                errorResponse = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            httpResponse.EnsureSuccessStatusCode();
            string content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            AuthenticationContext? response = JsonSerializer.Deserialize<AuthenticationContext>(json: content, options: ApplicationOptions.ApplicationSerializerOptions);
            result = response ?? throw new InvalidOperationException("Unable to deserialise authentication response.");
            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, "");
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }

        return result;
    }

    public async Task<UpdateStatementResponse> UpdateStatementAsync(Model.UpdateStatementRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(request.MaterialId);
        Requires.NotNull(request.WitnessId);
        Requires.NotNull(request.StatementNumber);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "UpdateStatement";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture))
                .Replace("{materialId}", request.MaterialId.ToString(CultureInfo.InvariantCulture));


            // Serialize the request object to JSON
            string jsonRequest = JsonSerializer.Serialize(new
            {
                request.CaseId,
                request.MaterialId,
                request.WitnessId,
                request.StatementDate,
                request.StatementNumber,
                request.Used
            });

            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, path)
            {
                Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            };

            UpdateStatementResponse? response = await this.HandleHttpRequestInternalAsync<UpdateStatementResponse>(
                httpRequest,
                cmsAuthValues,
                request,
                OperationName).ConfigureAwait(false);

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, $"materialId [{request.MaterialId}]");
            return response;
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }

    public async Task<UpdateExhibitResponse> UpdateExhibitAsync(UpdateExhibitRequest request, CmsAuthValues cmsAuthValues)
    {
        Requires.NotNull(request);
        Requires.NotNull(request.CaseId);
        Requires.NotNull(request.MaterialId);
        Requires.NotNull(request.DocumentType);
        Requires.NotNull(request.Item);
        Requires.NotNull(request.Reference);
        Requires.NotNull(cmsAuthValues);
        Requires.NotNull(cmsAuthValues.CmsCookies, nameof(cmsAuthValues.CmsCookies));
        Requires.NotNull(cmsAuthValues.CmsModernToken, nameof(cmsAuthValues.CmsModernToken));

        var stopwatch = Stopwatch.StartNew();
        const string OperationName = "UpdateExhibit";
        string path = "Undefined";
        string? errorResponse = null;

        try
        {
            HttpClient client = this.clientFactory.CreateClient(ClientEndpointOptions.DefaultSectionName);

            path = this.clientOptions.RelativePath[OperationName]
                .Replace("{caseId}", request.CaseId.ToString(CultureInfo.InvariantCulture))
                .Replace("{materialId}", request.MaterialId.ToString(CultureInfo.InvariantCulture));

            // Serialize the request object to JSON
            string jsonRequest = JsonSerializer.Serialize(new
            {
                request.CaseId,
                request.MaterialId,
                request.DocumentType,
                request.Item,
                request.Reference,
                request.Subject,
                request.Used,
                request.NewProducer,
                request.ExistingProducerOrWitnessId
            });

            var httpRequest = new HttpRequestMessage(HttpMethod.Patch, path)
            {
                Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            };

            UpdateExhibitResponse? response = await this.HandleHttpRequestInternalAsync<UpdateExhibitResponse>(
                httpRequest,
                cmsAuthValues,
                request,
                OperationName).ConfigureAwait(false);

            this.LogOperationCompletedEvent(OperationName, path, request, stopwatch.Elapsed, $"materialId [{request.MaterialId}]");
            return response;
        }
        catch (Exception exception)
        {
            this.HandleException(OperationName, path, errorResponse, exception, request, stopwatch.Elapsed);
            throw;
        }
    }
}
