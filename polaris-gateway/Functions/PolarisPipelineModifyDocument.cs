using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Request;
using Common.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Extensions;
using PolarisGateway.Mappers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;

namespace PolarisGateway.Functions;

public class PolarisPipelineModifyDocument : BaseFunction
{
    private readonly ILogger<PolarisPipelineModifyDocument> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private readonly IModifyDocumentRequestMapper _modifyDocumentRequestMapper;
    private readonly ITelemetryClient _telemetryClient;

    public PolarisPipelineModifyDocument(
        ILogger<PolarisPipelineModifyDocument> logger,
        ICoordinatorClient coordinatorClient,
        IModifyDocumentRequestMapper modifyDocumentRequestMapper,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
        _modifyDocumentRequestMapper = modifyDocumentRequestMapper ?? throw new ArgumentNullException(nameof(modifyDocumentRequestMapper));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
    }

    [Function(nameof(PolarisPipelineModifyDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ModifyDocument)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var telemetryEvent = new DocumentModifiedEvent(caseId, documentId);

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
                _telemetryClient.TrackEvent(telemetryEvent);
                return await new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                }.ToActionResult();
            }

            var modifyDocumentDto = _modifyDocumentRequestMapper.Map(documentChanges.Value);
            var response = await _coordinatorClient.ModifyDocument(
                caseUrn,
                caseId,
                documentId,
                versionId,
                modifyDocumentDto,
                cmsAuthValues,
                correlationId);

            telemetryEvent.IsSuccess = response.IsSuccessStatusCode;

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