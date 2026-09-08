// <copyright file="RedactDocument.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Functions;

using Common.Configuration;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Exceptions;
using Common.Extensions;
using Common.Services.BlobStorage;
using coordinator.Clients.PdfRedactor;
using coordinator.Clients.RedactionLogger;
using coordinator.Domain;
using coordinator.Services;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Services.CaseUrnResolver;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class RedactAndLog
{
    private readonly IValidator<RedactPdfRequestWithDocumentDto> requestValidator;
    private readonly IPolarisBlobStorageService polarisBlobStorageService;
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMdsClient mdsClient;
    private readonly ICaseUrnResolver caseUrnResolver;
    private readonly IRedactionService redactionService;
    private readonly IRedactionLoggerClient loggerClient;

    public RedactAndLog(
        IValidator<RedactPdfRequestWithDocumentDto> requestValidator,
        IRedactionService redactionService,
        IRedactionLoggerClient loggerClient,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IMdsArgFactory mdsArgFactory,
        IConfiguration configuration,
        IMdsClient mdsClient,
        ICaseUrnResolver caseUrnResolver)
    {
        this.requestValidator = requestValidator.ExceptionIfNull();
        this.redactionService = redactionService.ExceptionIfNull();
        this.loggerClient = loggerClient.ExceptionIfNull();
        this.polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty).ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.mdsClient = mdsClient.ExceptionIfNull();
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(RedactAndLog))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RedactAndLog)]
        HttpRequest req,
        int caseId,
        string materialId,
        long documentId,
        CancellationToken cancellationToken)
    {
        var correlationId = req.Headers.GetCorrelationId();
        var cmsAuthValues = req.BuildCmsAuthValues();

        var request =
            await req.ReadFromJsonAsync<RedactAndLogRequestDto>(
                cancellationToken);

        await this.redactionService.ProcessAsync(
            caseId,
            materialId,
            documentId,
            request!.RedactionPayload,
            cmsAuthValues,
            correlationId,
            cancellationToken);

        await this.loggerClient.CreateRedactionLog(
            request.LogPayload,
            correlationId);

        return new OkObjectResult(
            new RedactAndLogResponse
            {
                CorrelationId = correlationId,
                Success = true,
            });
    }
}
