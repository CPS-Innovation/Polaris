using Common.Configuration;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Helpers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class RenameDocument : BaseFunction
{
    private readonly ILogger<RenameDocument> _logger;
    private readonly ITelemetryClient _telemetryClient;
    private readonly IMdsClient _mdsClient;

    private const string ExhibitClassification = "EXHIBIT";
    private const string StatementClassification = "STATEMENT";

    public RenameDocument(ILogger<RenameDocument> logger,
        ITelemetryClient telemetryClient,
        IMdsClient mdsClient)
    {
        _logger = logger.ExceptionIfNull();
        _telemetryClient = telemetryClient.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    [Function(nameof(RenameDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RenameDocument)] HttpRequest req, string caseUrn, int caseId, string documentId)
    {
        var telemetryEvent = new RenameDocumentRequestEvent(caseId, documentId)
        {
            OperationName = nameof(RenameDocument),
        };

        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        try
        {
            telemetryEvent.IsRequestValid = true;
            telemetryEvent.CorrelationId = correlationId;

            var body = await RequestHelper.GetJsonBody<RenameDocumentRequestDto, RenameDocumentRequestValidator>(req);
            var isRequestJsonValid = body.IsValid;
            telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
            telemetryEvent.RequestJson = body.RequestJson;

            if (!isRequestJsonValid)
            {
                _telemetryClient.TrackEvent(telemetryEvent);
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var ddeiCaseIdentifiersArgDto = new DdeiCaseIdentifiersArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = caseUrn,
                CaseId = caseId,
            };
            var documents = await _mdsClient.ListDocumentsAsync(ddeiCaseIdentifiersArgDto);
            var documentIdNumber = DocumentNature.ToNumericDocumentId(documentId, DocumentNature.Types.Document);

            var document = documents.SingleOrDefault(x => x.DocumentId == documentIdNumber);

            if (document == null) return new NotFoundObjectResult("Document not found");

            var ddeiRenameDocumentArgDto = new DdeiRenameDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = caseUrn,
                CaseId = caseId,
                DocumentId = documentIdNumber,
                DocumentName = body.Value.DocumentName
            };
            if (string.Equals(document.Classification, ExhibitClassification, StringComparison.InvariantCultureIgnoreCase))
            {
                await _mdsClient.RenameExhibitAsync(ddeiRenameDocumentArgDto);
            }
            else if (!string.Equals(document.Classification, StatementClassification, StringComparison.InvariantCultureIgnoreCase))
            {
                await _mdsClient.RenameDocumentAsync(ddeiRenameDocumentArgDto);
            }

            telemetryEvent.IsSuccess = true;
            _telemetryClient.TrackEvent(telemetryEvent);

            return new OkResult();
        }
        catch
        {
            _telemetryClient.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}