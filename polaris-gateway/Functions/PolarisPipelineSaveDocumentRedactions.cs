using Common.Configuration;
using Common.Domain.Pii;
using Common.Dto.Request;
using Common.Telemetry;
using Common.Wrappers;
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
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class PolarisPipelineSaveDocumentRedactions : BaseFunction
{
    private readonly IRedactPdfRequestMapper _redactPdfRequestMapper;
    private readonly ILogger<PolarisPipelineSaveDocumentRedactions> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private readonly ITelemetryClient _telemetryClient;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;

    public PolarisPipelineSaveDocumentRedactions(
        IRedactPdfRequestMapper redactPdfRequestMapper,
        ICoordinatorClient coordinatorClient,
        ILogger<PolarisPipelineSaveDocumentRedactions> logger,
        ITelemetryClient telemetryClient,
        IJsonConvertWrapper jsonConvertWrapper)
        : base()

    {
        _redactPdfRequestMapper = redactPdfRequestMapper ?? throw new ArgumentNullException(nameof(redactPdfRequestMapper));
        _coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
    }

    [Function(nameof(PolarisPipelineSaveDocumentRedactions))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(PolarisPipelineSaveDocumentRedactions), tags: ["Documents"], Summary = "Polaris Pipeline Save Document Redactions", Description = "Gives the pdf")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the document", Required = true)]
    [OpenApiParameter("versionId", In = ParameterLocation.Path, Type = typeof(long), Description = "The version Id of the document", Required = true)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<PiiLine>), Description = "OCR processing completed successfully")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RedactDocument)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var telemetryEvent = new RedactionRequestEvent(caseId, documentId)
        {
            OperationName = nameof(PolarisPipelineSaveDocumentRedactions),
        };

        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

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
                // todo: log these errors to telemetry event
                _telemetryClient.TrackEvent(telemetryEvent);
                return await new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                }.ToActionResult();
            }

            var redactPdfRequest = _redactPdfRequestMapper.Map(redactions.Value);
            var response = await _coordinatorClient.SaveRedactionsAsync(
                caseUrn,
                caseId,
                documentId,
                versionId,
                redactPdfRequest,
                cmsAuthValues,
                correlationId);

            telemetryEvent.IsSuccess = response.IsSuccessStatusCode;
            telemetryEvent.DeletedPageCount = redactPdfRequest.DocumentModifications.Count;

            _telemetryClient.TrackEvent(telemetryEvent);
            return await response.ToActionResult();
        }
        catch
        {
            _telemetryClient.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}