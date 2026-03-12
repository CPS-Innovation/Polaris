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
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
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
    [OpenApiOperation(operationId: nameof(RenameDocument), tags: ["Documents"], Summary = "Rename Document", Description = "Rename Document")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the document", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Document rename", Description = "Returns list of document notes")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

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

            var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = caseUrn,
                CaseId = caseId,
            };
            var documents = await _mdsClient.ListDocumentsAsync(mdsCaseIdentifiersArgDto);
            var documentIdNumber = DocumentNature.ToNumericDocumentId(documentId, DocumentNature.Types.Document);

            var document = documents.SingleOrDefault(x => x.DocumentId == documentIdNumber);

            if (document == null) return new NotFoundObjectResult("Document not found");

            var mdsRenameDocumentArgDto = new MdsRenameDocumentArgDto
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
                await _mdsClient.RenameExhibitAsync(mdsRenameDocumentArgDto);
            }
            else if (!string.Equals(document.Classification, StatementClassification, StringComparison.InvariantCultureIgnoreCase))
            {
                await _mdsClient.RenameDocumentAsync(mdsRenameDocumentArgDto);
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