// <copyright file="PolarisPipelineModifyDocument.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Extensions;
using PolarisGateway.Mappers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;

public class PolarisPipelineModifyDocument : BaseFunction
{
    private readonly ILogger<PolarisPipelineModifyDocument> logger;
    private readonly ICoordinatorClient coordinatorClient;
    private readonly IModifyDocumentRequestMapper modifyDocumentRequestMapper;

    public PolarisPipelineModifyDocument(
        ILogger<PolarisPipelineModifyDocument> logger,
        ICoordinatorClient coordinatorClient,
        IModifyDocumentRequestMapper modifyDocumentRequestMapper)
        : base()
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
        this.modifyDocumentRequestMapper = modifyDocumentRequestMapper ?? throw new ArgumentNullException(nameof(modifyDocumentRequestMapper));
    }

    [Function(nameof(PolarisPipelineModifyDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(PolarisPipelineModifyDocument), tags: ["Case"], Summary = "Polaris Pipeline Modify Document", Description = "Returns case information using caseURN and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ModifyDocument)] HttpRequest req, int caseId, string materialId, long documentId, CancellationToken cancellationToken = default)
    {
        var telemetryEvent = new DocumentModifiedEvent(caseId, materialId)
        {
            OperationName = nameof(PolarisPipelineModifyDocument),
        };

        cancellationToken.ThrowIfCancellationRequested();
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        try
        {
            telemetryEvent.IsRequestValid = true;
            telemetryEvent.CorrelationId = correlationId;

            var documentChanges = await ValidatorHelper.GetJsonBody<DocumentModificationRequestDto, ModifyDocumentPagesValidator>(req);
            var isRequestJsonValid = documentChanges.IsValid;
            telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
            telemetryEvent.RequestJson = documentChanges.RequestJson;

            if (!isRequestJsonValid)
            {
                this.logger.TrackEvent(telemetryEvent);
                return await new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                }.ToActionResult();
            }

            var modifyDocumentDto = this.modifyDocumentRequestMapper.Map(documentChanges.Value);
            var response = await this.coordinatorClient.ModifyDocument(
                caseUrn: null,
                caseId,
                materialId,
                documentId,
                modifyDocumentDto,
                cmsAuthValues.CmsAuthFullValue,
                correlationId,
                isLegacy: false);

            telemetryEvent.IsSuccess = response.IsSuccessStatusCode;

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
