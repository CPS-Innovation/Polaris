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
    private readonly IPdfRedactorClient redactionClient;
    private readonly IPolarisBlobStorageService polarisBlobStorageService;
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMdsClient mdsClient;
    private readonly ICaseUrnResolver caseUrnResolver;
    private readonly IRedactionLoggerClient loggerClient;

    public RedactAndLog(
        IValidator<RedactPdfRequestWithDocumentDto> requestValidator,
        IPdfRedactorClient redactionClient,
        IRedactionLoggerClient loggerClient,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IMdsArgFactory mdsArgFactory,
        IConfiguration configuration,
        IMdsClient mdsClient,
        ICaseUrnResolver caseUrnResolver)
    {
        this.requestValidator = requestValidator.ExceptionIfNull();
        this.redactionClient = redactionClient.ExceptionIfNull();
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
        var idempotencyKey = correlationId; // Use the same key across downstream calls

        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var request = await req.ReadFromJsonAsync<RedactAndLogRequestDto>(cancellationToken);
        var redactPdfRequest = request!.RedactionPayload;

        var response = new RedactAndLogResponse
        {
            CorrelationId = correlationId,
            Redaction = new StepResult { Status = "NotStarted" },
            Logging = new StepResult { Status = "NotStarted" },
        };

        try
        {
            var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(
                caseId,
                cmsAuthValues,
                cancellationToken);

            using var documentStream = await this.polarisBlobStorageService.GetBlobAsync(
                new BlobIdType(caseId, materialId, documentId, BlobType.Pdf));

            using var memoryStream = new MemoryStream();
            await documentStream.CopyToAsync(memoryStream, cancellationToken);

            var bytes = memoryStream.ToArray();

            Stream redactedDocumentStream = null;
            Stream modifiedDocumentStream = null;

            // ---------------------------------------------------------
            // STEP 1 - REDACTION
            // ---------------------------------------------------------
            if (redactPdfRequest.RedactionDefinitions.Count != 0)
            {
                var redactionRequest = new RedactPdfRequestWithDocumentDto
                {
                    Document = Convert.ToBase64String(bytes),
                    RedactionDefinitions = redactPdfRequest.RedactionDefinitions,
                };

                var validationResult = await this.requestValidator.ValidateAsync(
                    redactionRequest,
                    cancellationToken);

                if (!validationResult.IsValid)
                {
                    response = new RedactAndLogResponse
                    {
                        Redaction = new StepResult
                        {
                            Status = "Failed",
                            Error = validationResult.FlattenErrors(),
                        },
                    };

                    return new BadRequestObjectResult(response);
                }

                try
                {
                    redactedDocumentStream =
                        await this.redactionClient.RedactPdfAsync(
                            caseUrn: null,
                            caseId,
                            materialId,
                            documentId,
                            redactionRequest,
                            correlationId,
                            isLegacy: false,
                            idempotencyKey: idempotencyKey);

                    if (redactedDocumentStream == null)
                    {
                        throw new InvalidOperationException(
                            $"Error saving redaction details for {caseId}, materialId {materialId}");
                    }

                    response = new RedactAndLogResponse
                    {
                        Redaction = new StepResult
                        {
                            Status = "Succeeded",
                        },
                    };
                }
                catch (Exception ex)
                {
                    response = new RedactAndLogResponse
                    {
                        Redaction = new StepResult
                        {
                            Status = "Failed",
                            Error = ex.Message,
                        },
                    };

                    return new ObjectResult(response)
                    {
                        StatusCode = StatusCodes.Status502BadGateway,
                    };
                }
            }
            else
            {
                response = new RedactAndLogResponse
                {
                    Redaction = new StepResult
                    {
                        Status = "Skipped",
                    },
                };
            }

            // ---------------------------------------------------------
            // STEP 2 - DOCUMENT MODIFICATION
            // ---------------------------------------------------------
            if (redactPdfRequest.DocumentModifications.Count != 0)
            {
                byte[] bytesToModify;

                if (redactedDocumentStream != null)
                {
                    using var redactedMemoryStream = new MemoryStream();

                    // Ensure the stream can be read from the beginning.
                    if (redactedDocumentStream.CanSeek)
                    {
                        redactedDocumentStream.Position = 0;
                    }

                    await redactedDocumentStream.CopyToAsync(
                        redactedMemoryStream,
                        cancellationToken);

                    bytesToModify = redactedMemoryStream.ToArray();
                }
                else
                {
                    bytesToModify = bytes;
                }

                var modificationRequest = new ModifyDocumentWithDocumentDto
                {
                    Document = Convert.ToBase64String(bytesToModify),
                    DocumentModifications = redactPdfRequest.DocumentModifications,
                    VersionId = redactPdfRequest.VersionId,
                };

                modifiedDocumentStream =
                    await this.redactionClient.ModifyDocument(
                        caseUrn,
                        caseId,
                        materialId,
                        documentId,
                        modificationRequest,
                        correlationId,
                        idempotencyKey);

                if (modifiedDocumentStream == null)
                {
                    throw new InvalidOperationException(
                        $"Error modifying document for {caseId}, materialId {materialId}");
                }
            }

            // ---------------------------------------------------------
            // STEP 3 - UPLOAD DOCUMENT
            // ---------------------------------------------------------
            var arg = this.mdsArgFactory.CreateDocumentVersionArgDto(
                cmsAuthValues.CmsAuthFullValue,
                correlationId: correlationId,
                caseUrn,
                caseId: caseId,
                DocumentNature.ToNumericDocumentId(
                    materialId,
                    DocumentNature.Types.Document),
                documentId);

            var documentToUpload =
                modifiedDocumentStream ??
                redactedDocumentStream;

            var ddeiResult = await this.mdsClient.UploadPdfAsync(
                arg,
                documentToUpload,
                cancellationToken);

            if (ddeiResult.StatusCode == HttpStatusCode.Gone ||
                ddeiResult.StatusCode == HttpStatusCode.RequestEntityTooLarge)
            {
                response = new RedactAndLogResponse
                {
                    Logging = new StepResult
                    {
                        Status = "NotStarted",
                    },
                };

                return new ObjectResult(response)
                {
                    StatusCode = (int)ddeiResult.StatusCode,
                };
            }

            // ---------------------------------------------------------
            // STEP 4 - LOGGING
            // ---------------------------------------------------------
            try
            {
                var createRedactionLogsRequest =
                    request.LogPayload;

                await this.loggerClient.CreateRedactionLog(
                    createRedactionLogsRequest,
                    correlationId,
                    idempotencyKey);

                response = new RedactAndLogResponse
                {
                    Logging = new StepResult
                    {
                        Status = "Succeeded",
                    },
                };
            }
            catch (Exception ex)
            {
                response = new RedactAndLogResponse
                {
                    Logging = new StepResult
                    {
                        Status = "Failed",
                        Error = ex.Message,
                    },
                };

                return new ObjectResult(response)
                {
                    StatusCode = StatusCodes.Status502BadGateway,
                };
            }

            // ---------------------------------------------------------
            // SUCCESS - ONLY AFTER BOTH STEPS SUCCEED
            // ---------------------------------------------------------
            response = new RedactAndLogResponse
            {
                Success = true,
            };

            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            response = new RedactAndLogResponse
            {
                Success = false,
                Logging = new StepResult
                {
                    Status = "Failed",
                    Error = ex.Message,
                },
            };

            return new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }
    }
}
