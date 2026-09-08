// <copyright file="ReclassifyDocument.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using Ddei.Factories;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Services.MdsOrchestration;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class ReclassifyDocument : BaseFunction
{
    private readonly ILogger<ReclassifyDocument> logger;
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMdsReclassifyDocumentOrchestrationService mdsOrchestrationService;
    private readonly ICaseUrnResolver caseUrnResolver;

    public ReclassifyDocument(
        ILogger<ReclassifyDocument> logger,
        IMdsArgFactory mdsArgFactory,
        IMdsReclassifyDocumentOrchestrationService mdsOrchestrationService,
        ICaseUrnResolver caseUrnResolver)
        : base()
    {
        this.logger = logger.ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.mdsOrchestrationService = mdsOrchestrationService.ExceptionIfNull();
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(ReclassifyDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation(operationId: nameof(ReclassifyDocument), tags: ["Documents"], Summary = "Reclassify Document", Description = "Reclassify Document")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Document Note List", Description = "Returns list of document notes")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ReclassifyDocument)] HttpRequest req, int caseId, string materialId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var telemetryEvent = new DocumentReclassifiedEvent(caseId, materialId)
        {
            OperationName = nameof(ReclassifyDocument),
        };

        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

        try
        {
            telemetryEvent.IsRequestValid = true;
            telemetryEvent.CorrelationId = correlationId;

            var body = await ValidatorHelper.GetJsonBody<ReclassifyDocumentDto, ReclassifyDocumentValidator>(req);
            telemetryEvent.IsRequestJsonValid = body.IsValid;
            telemetryEvent.RequestJson = body.RequestJson;

            if (!body.IsValid)
            {
                this.logger.TrackEvent(telemetryEvent);
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var arg = this.mdsArgFactory.CreateReclassifyDocumentArgDto
            (
                cmsAuthValues: cmsAuthValues.CmsAuthFullValue,
                correlationId: correlationId,
                urn: caseUrn,
                caseId: caseId,
                materialId: materialId,
                dto: body.Value
            );

            var reclassifyDocumentResult = await this.mdsOrchestrationService.ReclassifyDocument(arg);

            if (!reclassifyDocumentResult.IsSuccess)
            {
                telemetryEvent.IsSuccess = false;
                this.logger.TrackEvent(telemetryEvent);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            telemetryEvent.IsSuccess = true;
            telemetryEvent.ResponseDocumentId = (int)reclassifyDocumentResult.Result.DocumentId;
            telemetryEvent.ReclassificationType = reclassifyDocumentResult.Result.ReclassificationType;
            telemetryEvent.OriginalDocumentTypeId = reclassifyDocumentResult.Result.OriginalDocumentTypeId;
            telemetryEvent.NewDocumentTypeId = reclassifyDocumentResult.Result.DocumentTypeId;
            telemetryEvent.DocumentRenamed = reclassifyDocumentResult.Result.DocumentRenamed;
            telemetryEvent.DocumentRenameOperationName = reclassifyDocumentResult.Result.DocumentRenamedOperationName;
            this.logger.TrackEvent(telemetryEvent);

            return new ObjectResult(reclassifyDocumentResult.Result);
        }
        catch
        {
            this.logger.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}
