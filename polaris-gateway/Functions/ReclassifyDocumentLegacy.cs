using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using Ddei.Factories;
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

namespace PolarisGateway.Functions;

public class ReclassifyDocumentLegacy : BaseFunction
{
    private readonly ILogger<ReclassifyDocumentLegacy> _logger;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly IMdsReclassifyDocumentOrchestrationService _mdsOrchestrationService;

    public ReclassifyDocumentLegacy(
        ILogger<ReclassifyDocumentLegacy> logger,
        IMdsArgFactory mdsArgFactory,
        IMdsReclassifyDocumentOrchestrationService mdsOrchestrationService)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _mdsOrchestrationService = mdsOrchestrationService.ExceptionIfNull();
    }

    [Function(nameof(ReclassifyDocumentLegacy))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation(operationId: nameof(ReclassifyDocumentLegacy), tags: ["Documents"], Summary = "Reclassify Document", Description = "Reclassify Document")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Document Note List", Description = "Returns list of document notes")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ReclassifyDocumentLegacy)] HttpRequest req, string caseUrn, int caseId, string materialId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var telemetryEvent = new DocumentReclassifiedEvent(caseId, materialId)
        {
            OperationName = nameof(ReclassifyDocumentLegacy),
        };

        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        try
        {
            telemetryEvent.IsRequestValid = true;
            telemetryEvent.CorrelationId = correlationId;

            var body = await ValidatorHelper.GetJsonBody<ReclassifyDocumentDto, ReclassifyDocumentValidator>(req);
            telemetryEvent.IsRequestJsonValid = body.IsValid;
            telemetryEvent.RequestJson = body.RequestJson;

            if (!body.IsValid)
            {
                _logger.TrackEvent(telemetryEvent);
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var arg = _mdsArgFactory.CreateReclassifyDocumentArgDto
            (
                cmsAuthValues: cmsAuthValues,
                correlationId: correlationId,
                urn: caseUrn,
                caseId: caseId,
                materialId: materialId,
                dto: body.Value
            );

            var reclassifyDocumentResult = await _mdsOrchestrationService.ReclassifyDocument(arg);

            if (!reclassifyDocumentResult.IsSuccess)
            {
                telemetryEvent.IsSuccess = false;
                _logger.TrackEvent(telemetryEvent);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            telemetryEvent.IsSuccess = true;
            telemetryEvent.ResponseDocumentId = (int)reclassifyDocumentResult.Result.DocumentId;
            telemetryEvent.ReclassificationType = reclassifyDocumentResult.Result.ReclassificationType;
            telemetryEvent.OriginalDocumentTypeId = reclassifyDocumentResult.Result.OriginalDocumentTypeId;
            telemetryEvent.NewDocumentTypeId = reclassifyDocumentResult.Result.DocumentTypeId;
            telemetryEvent.DocumentRenamed = reclassifyDocumentResult.Result.DocumentRenamed;
            telemetryEvent.DocumentRenameOperationName = reclassifyDocumentResult.Result.DocumentRenamedOperationName;
            _logger.TrackEvent(telemetryEvent);

            return new ObjectResult(reclassifyDocumentResult.Result);
        }
        catch
        {
            _logger.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}