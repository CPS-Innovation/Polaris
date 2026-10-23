// <copyright file="AddDocumentNote.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Helpers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class AddDocumentNote : BaseFunction
{
    private readonly ILogger<AddDocumentNote> logger;
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMdsClient mdsClient;
    private readonly ICaseUrnResolver caseUrnResolver;

    public AddDocumentNote(
        ILogger<AddDocumentNote> logger,
        IMdsArgFactory mdsArgFactory,
        IMdsClient mdsClient,
        ICaseUrnResolver caseUrnResolver)
    {
        this.logger = logger.ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.mdsClient = mdsClient.ExceptionIfNull();
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(AddDocumentNote))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(AddDocumentNote), tags: ["Documents"], Summary = "Add Document Note", Description = "Returns case information using caseUrl and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material to which the note has to be added", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddDocumentNoteRequestDto), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentNotes)] HttpRequest req,
        int caseId,
        string materialId,
        CancellationToken cancellationToken = default)
    {
        var telemetryEvent = new DocumentNoteRequestEvent(caseId, materialId)
        {
            OperationName = nameof(AddDocumentNote),
        };
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

        try
        {
            telemetryEvent.IsRequestValid = true;
            telemetryEvent.CorrelationId = correlationId;

            var body = await RequestHelper.GetJsonBody<AddDocumentNoteRequestDto, AddDocumentNoteValidator>(req);

            telemetryEvent.IsRequestJsonValid = body.IsValid;
            telemetryEvent.RequestJson = body.RequestJson;

            if (!body.IsValid)
            {
                this.logger.TrackEvent(telemetryEvent);
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var arg = this.mdsArgFactory.CreateAddDocumentNoteArgDto(cmsAuthValues.CmsAuthFullValue, correlationId, caseUrn, caseId, materialId, body.Value.Text);

            await this.mdsClient.AddDocumentNoteAsync(arg, cancellationToken);

            telemetryEvent.IsSuccess = true;
            this.logger.TrackEvent(telemetryEvent);

            return new OkResult();
        }
        catch
        {
            this.logger.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}
