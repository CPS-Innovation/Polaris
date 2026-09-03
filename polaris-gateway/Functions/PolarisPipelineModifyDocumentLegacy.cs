using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Request;
using Common.Telemetry;
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

namespace PolarisGateway.Functions;

public class PolarisPipelineModifyDocumentLegacy : BaseFunction
{
    private readonly ILogger<PolarisPipelineModifyDocumentLegacy> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private readonly IModifyDocumentRequestMapper _modifyDocumentRequestMapper;

    public PolarisPipelineModifyDocumentLegacy(
        ILogger<PolarisPipelineModifyDocumentLegacy> logger,
        ICoordinatorClient coordinatorClient,
        IModifyDocumentRequestMapper modifyDocumentRequestMapper)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
        _modifyDocumentRequestMapper = modifyDocumentRequestMapper ?? throw new ArgumentNullException(nameof(modifyDocumentRequestMapper));
    }

    [Function(nameof(PolarisPipelineModifyDocumentLegacy))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(PolarisPipelineModifyDocumentLegacy), tags: ["Case"], Summary = "Polaris Pipeline Modify Document", Description = "Returns case information using caseURN and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ModifyDocumentLegacy)] HttpRequest req, string caseUrn, int caseId, string materialId, long documentId, CancellationToken cancellationToken = default)
    {
        var telemetryEvent = new DocumentModifiedEvent(caseId, materialId)
        {
            OperationName = nameof(PolarisPipelineModifyDocumentLegacy),
        };

        cancellationToken.ThrowIfCancellationRequested();
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

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
                _logger.TrackEvent(telemetryEvent);
                return await new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                }.ToActionResult();
            }

            var modifyDocumentDto = _modifyDocumentRequestMapper.Map(documentChanges.Value);
            var response = await _coordinatorClient.ModifyDocument(
                caseUrn,
                caseId,
                materialId,
                documentId,
                modifyDocumentDto,
                cmsAuthValues,
                correlationId, 
                isLegacy: true);

            telemetryEvent.IsSuccess = response.IsSuccessStatusCode;

            _logger.TrackEvent(telemetryEvent);
            return await response.ToActionResult();
        }
        catch
        {
            _logger.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}