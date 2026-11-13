namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using Ddei.Factories;
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class AddDocumentNoteLegacy : BaseFunction
{
    private readonly ILogger<AddDocumentNoteLegacy> logger;
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMasterDataServiceClient masterDataServiceClient;

    public AddDocumentNoteLegacy(
        ILogger<AddDocumentNoteLegacy> logger,
        IMdsArgFactory mdsArgFactory,
        IMdsClient mdsClient,
        IMasterDataServiceClient masterDataServiceClient)
    {
        this.logger = logger.ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
    }

    [Function(nameof(AddDocumentNoteLegacy))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(AddDocumentNoteLegacy), tags: ["Documents"], Summary = "Add Document Note", Description = "Returns case information using caseUrl and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material to which the note has to be added", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddDocumentNoteRequestDto), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentNotesLegacy)] HttpRequest req,
        string caseUrn,
        int caseId,
        string materialId,
        CancellationToken cancellationToken = default)
    {
        var telemetryEvent = new DocumentNoteRequestEvent(caseId, materialId)
        {
            OperationName = nameof(AddDocumentNoteLegacy),
        };
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

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

            var arg = this.mdsArgFactory.CreateAddDocumentNoteArgDto(cmsAuthValues, correlationId, caseUrn, caseId, materialId, body.Value.Text);

            await this.masterDataServiceClient.CreateDocumentNoteAsync(arg, new CmsAuthValues(cmsAuthValues, correlationId), cancellationToken);

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
