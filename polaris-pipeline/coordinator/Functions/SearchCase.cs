// <copyright file="SearchCase.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Functions;

using System.Linq;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Telemetry;
using coordinator.TelemetryEvents;
using coordinator.Helpers;
using coordinator.Mappers;
using coordinator.Durable.Payloads.Domain;
using Microsoft.AspNetCore.Http;
using coordinator.Clients.TextExtractor;
using Microsoft.Azure.Functions.Worker;
using coordinator.Domain;
using Common.Services.BlobStorage;
using System;
using Microsoft.Extensions.Configuration;
using DdeiClient.Services.CaseUrnResolver;
using System.Threading;
using Common.Dto.Request;

public class SearchCase
{
    private const string QueryStringSearchParam = "query";
    private readonly ITextExtractorClient textExtractorClient;
    private readonly ISearchFilterDocumentMapper searchFilterDocumentMapper;
    private readonly IPolarisBlobStorageService polarisBlobStorageService;
    private readonly ICaseUrnResolver caseUrnResolver;
    private readonly ITelemetryClient telemetryClient;

    public SearchCase(
        IConfiguration configuration,
        ITextExtractorClient textExtractorClient,
        ISearchFilterDocumentMapper searchFilterDocumentMapper,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        ICaseUrnResolver caseUrnResolver,
        ITelemetryClient telemetryClient)
    {
        this.textExtractorClient = textExtractorClient;
        this.searchFilterDocumentMapper = searchFilterDocumentMapper;
        this.polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
        this.caseUrnResolver = caseUrnResolver;
        this.telemetryClient = telemetryClient;
    }

    [Function(nameof(SearchCase))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseSearch)] HttpRequest req,
        int caseId,
        CancellationToken cancellation)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();
        var searchTerm = req.Query[QueryStringSearchParam];

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new BadRequestObjectResult("Search term not supplied.");
        }

        var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellation);
        var searchResults = await this.textExtractorClient.SearchTextAsync(caseUrn, caseId, searchTerm, currentCorrelationId);

        var documentStateBlobId = new BlobIdType(caseId, default, default, BlobType.DocumentState);
        var documentsState = (await this.polarisBlobStorageService.TryGetObjectAsync<CaseDurableEntityDocumentsState>(documentStateBlobId)) ?? new CaseDurableEntityDocumentsState();

        // todo: temporary code, need an AllDocuments method as per first refactor
        var documents =
            documentsState.CmsDocuments.OfType<BaseDocumentEntity>()
                .Concat(documentsState.PcdRequests)
                .Append(documentsState.DefendantsAndCharges)
                .Select(this.searchFilterDocumentMapper.MapToSearchFilterDocument)
                .ToList();

        var filteredSearchResults = searchResults
            .Where(result => documents.Any(doc => doc.DocumentId == result.DocumentId && doc.VersionId == result.VersionId))
            .ToList();

        var documentIds = filteredSearchResults
            .Select(result => result.DocumentId)
            .Distinct()
            .ToList();

        // the max string length of Application Insights custom properties is 8192
        // so we chunk the docIds and create multiple events as some cases could exceed this limit
        var chunkedDocumentIds = ChunkHelper.ChunkStringListByMaxCharacterCount(documentIds, 8192);

        foreach (var documentIdsChunk in chunkedDocumentIds)
        {
            var telemetryEvent = new SearchCaseEvent(
                currentCorrelationId,
                caseId,
                documentIdsChunk)
            {
                OperationName = nameof(SearchCase),
            };
            this.telemetryClient.TrackEvent(telemetryEvent);
        }

        return new OkObjectResult(filteredSearchResults);
    }
}
