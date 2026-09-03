// <copyright file="PolarisPipelineSaveDocumentRedactions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Domain.Pii;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using Common.Wrappers;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Extensions;
using PolarisGateway.Helpers;
using PolarisGateway.Mappers;
using PolarisGateway.Models;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class PolarisPipelineSaveDocumentRedactions : BaseFunction
{
    private readonly IRedactPdfRequestMapper redactPdfRequestMapper;
    private readonly ILogger<PolarisPipelineSaveDocumentRedactions> logger;
    private readonly ICoordinatorClient coordinatorClient;

    public PolarisPipelineSaveDocumentRedactions(
        IRedactPdfRequestMapper redactPdfRequestMapper,
        ICoordinatorClient coordinatorClient,
        ILogger<PolarisPipelineSaveDocumentRedactions> logger)
        : base()

    {
        this.redactPdfRequestMapper = redactPdfRequestMapper ?? throw new ArgumentNullException(nameof(redactPdfRequestMapper));
        this.coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function(nameof(PolarisPipelineSaveDocumentRedactions))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(PolarisPipelineSaveDocumentRedactions), tags: ["Documents"], Summary = "Polaris Pipeline Save Document Redactions", Description = "Gives the pdf")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(long), Description = "The document Id (version) of the material", Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<PiiLine>), Description = "OCR processing completed successfully")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RedactDocument)] HttpRequest req, int caseId, string materialId, long documentId, CancellationToken cancellationToken = default)
    {
        var telemetryEvent = new RedactionRequestEvent(caseId, materialId)
        {
            OperationName = nameof(PolarisPipelineSaveDocumentRedactions),
        };

        cancellationToken.ThrowIfCancellationRequested();
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        try
        {
            telemetryEvent.IsRequestValid = true;
            telemetryEvent.CorrelationId = correlationId;

            var redactions = await RequestHelper.GetJsonBody<DocumentRedactionSaveRequestDto, DocumentRedactionSaveRequestValidator>(req);
            var isRequestJsonValid = redactions.IsValid;
            telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
            telemetryEvent.RequestJson = redactions.RequestJson;

            if (!isRequestJsonValid)
            {
                this.logger.TrackEvent(telemetryEvent);
                return await new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                }.ToActionResult();
            }

            var redactPdfRequest = this.redactPdfRequestMapper.Map(redactions.Value);
            var response = await this.coordinatorClient.SaveRedactionsAsync(
                caseUrn: null,
                caseId,
                materialId,
                documentId,
                redactPdfRequest,
                cmsAuthValues.CmsAuthFullValue,
                correlationId,
                isLegacy: false);

            telemetryEvent.IsSuccess = response.IsSuccessStatusCode;
            telemetryEvent.DeletedPageCount = redactPdfRequest.DocumentModifications.Count;

            this.logger.TrackEvent(telemetryEvent);
            return await response.ToActionResult();
        }
        catch
        {
            this.logger.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}
