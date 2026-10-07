using Common.Configuration;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Extensions;
using Common.Mappers;
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
using PolarisGateway.Services.MdsOrchestration.Mappers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class RenameDocumentLegacy : BaseFunction
{
    private readonly ILogger<RenameDocumentLegacy> _logger;
    private readonly IMdsClient _mdsClient;
    private readonly IMasterDataServiceClient _masterDataServiceClient;
    private readonly ICmsDocumentDtoMapper _cmsDocumentDtoMapper;

    private const string ExhibitClassification = "EXHIBIT";
    private const string StatementClassification = "STATEMENT";

    public RenameDocumentLegacy(ILogger<RenameDocumentLegacy> logger,
        IMdsClient mdsClient, 
        IMasterDataServiceClient masterDataServiceClient,
        ICmsDocumentDtoMapper cmsDocumentDtoMapper)
    {
        _logger = logger.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
        _masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
        _cmsDocumentDtoMapper = cmsDocumentDtoMapper.ExceptionIfNull();
    }

    [Function(nameof(RenameDocumentLegacy))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(RenameDocumentLegacy), tags: ["Documents"], Summary = "Rename Document", Description = "Rename Document")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Document rename", Description = "Returns list of document notes")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RenameDocumentLegacy)] HttpRequest req, string caseUrn, int caseId, string materialId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var telemetryEvent = new RenameDocumentRequestEvent(caseId, materialId)
        {
            OperationName = nameof(RenameDocumentLegacy),
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
                _logger.TrackEvent(telemetryEvent);
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var mdsCaseIdentifiersArgDto = new MdsCaseIdentifiersArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = caseUrn,
                CaseId = caseId,
            };
            //var documentsOld = await _mdsClient.ListDocumentsAsync(mdsCaseIdentifiersArgDto);
            var documentsResponse = await _masterDataServiceClient.ListDocumentsAsync(mdsCaseIdentifiersArgDto, new CmsAuthValues(cmsAuthValues, correlationId), cancellationToken);
            var documents = documentsResponse.Select(x => _cmsDocumentDtoMapper.Map(x, null)).ToList();
            var documentIdNumber = DocumentNature.ToNumericDocumentId(materialId, DocumentNature.Types.Document);

            var document = documents.SingleOrDefault(x => x.DocumentId == documentIdNumber);

            if (document == null) return new NotFoundObjectResult("Document not found");

            var mdsRenameDocumentArgDto = new MdsRenameDocumentArgDto
            {
                CmsAuthValues = cmsAuthValues,
                CorrelationId = correlationId,
                Urn = caseUrn,
                CaseId = caseId,
                MaterialId = documentIdNumber,
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
            _logger.TrackEvent(telemetryEvent);

            return new OkResult();
        }
        catch
        {
            _logger.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}