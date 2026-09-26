// <copyright file="RedactAndLog.cs" company="TheCrownProsecutionService">
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
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

public class RedactAndLog
{
    private readonly IValidator<RedactPdfRequestDto> requestValidator;
    private readonly IRedactionService redactionService;
    private readonly IRedactionLoggerClient loggerClient;
    private readonly ILogger<RedactAndLog> logger;

    public RedactAndLog(
        IValidator<RedactPdfRequestDto> requestValidator,
        IRedactionService redactionService,
        IRedactionLoggerClient loggerClient,
        IConfiguration configuration,
        ILogger<RedactAndLog> logger)
    {
        this.requestValidator = requestValidator.ExceptionIfNull();
        this.redactionService = redactionService.ExceptionIfNull();
        this.loggerClient = loggerClient.ExceptionIfNull();
        this.logger = logger.ExceptionIfNull();
    }

    [Function(nameof(RedactAndLog))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        var authorizationHeader = req.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !AuthenticationHeaderValue.TryParse(authorizationHeader, out var authHeader) ||
            !string.Equals(authHeader.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase))
        {
            return new UnauthorizedResult();
        }

        try
        {
            var request =
                await req.ReadFromJsonAsync<RedactAndLogRequestDto>(
                    cancellationToken);

            if (request is null)
            {
                this.logger.LogWarning(
                    "RedactAndLog request was empty. CorrelationId: {CorrelationId}, CaseId: {CaseId}, MaterialId: {MaterialId}, DocumentId: {DocumentId}",
                    correlationId,
                    caseId,
                    materialId,
                    documentId);

                return new BadRequestObjectResult(
                    new RedactAndLogResponse
                    {
                        CorrelationId = correlationId,
                        Success = false,
                    });
            }

            var validationResult = await this.requestValidator.ValidateAsync(
                                        request.RedactionPayload,
                                        cancellationToken);

            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(
                    new RedactAndLogResponse
                    {
                        CorrelationId = correlationId,
                        Success = false,
                    });
            }

            await this.redactionService.ProcessAsync(
                caseId,
                materialId,
                documentId,
                request.RedactionPayload,
                cmsAuthValues,
                correlationId,
                cancellationToken);

            this.logger.LogInformation(
                "Redaction completed successfully. CorrelationId: {CorrelationId}, CaseId: {CaseId}, MaterialId: {MaterialId}, DocumentId: {DocumentId}",
                correlationId,
                caseId,
                materialId,
                documentId);

            await this.loggerClient.CreateRedactionLog(
                request.LogPayload,
                correlationId);

            this.logger.LogInformation(
                "Redaction log created successfully. CorrelationId: {CorrelationId}, CaseId: {CaseId}, MaterialId: {MaterialId}, DocumentId: {DocumentId}",
                correlationId,
                caseId,
                materialId,
                documentId);

            return new OkObjectResult(
                new RedactAndLogResponse
                {
                    CorrelationId = correlationId,
                    Success = true,
                });
        }
        catch (OperationCanceledException opex) when (cancellationToken.IsCancellationRequested)
        {
            this.logger.LogWarning(
                opex,
                "RedactAndLog operation was cancelled. CorrelationId: {CorrelationId}, CaseId: {CaseId}, MaterialId: {MaterialId}, DocumentId: {DocumentId}",
                correlationId,
                caseId,
                materialId,
                documentId);

            throw new OperationCanceledException(
                    $"RedactAndLog operation was cancelled. CorrelationId: {correlationId}, " +
                    $"CaseId: {caseId}, MaterialId: {materialId}, DocumentId: {documentId}",
                    opex,
                    cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "RedactAndLog failed. CorrelationId: {CorrelationId}, CaseId: {CaseId}, MaterialId: {MaterialId}, DocumentId: {DocumentId}",
                correlationId,
                caseId,
                materialId,
                documentId);

            return new ObjectResult(
               new RedactAndLogResponse
               {
                   CorrelationId = correlationId,
                   Success = false,
                   Redaction = new StepResult
                   {
                       Status = "Failed",
                       Error = ex.Message,
                   },
                   Logging = null,
               })
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }
    }
}
