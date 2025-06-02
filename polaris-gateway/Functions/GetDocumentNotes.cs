using Common.Configuration;
using Common.Extensions;
using Common.Telemetry;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetDocumentNotes : BaseFunction
{
    private readonly ILogger<GetDocumentNotes> _logger;
    private readonly IDdeiClientFactory _ddeiClientFactory;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly ITelemetryClient _telemetryClient;

    public GetDocumentNotes(ILogger<GetDocumentNotes> logger,
        IDdeiClientFactory ddeiClientFactory,
        [FromKeyedServices(DdeiClients.Ddei)] IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(GetDocumentNotes))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.DocumentNotes)] HttpRequest req, string caseUrn, int caseId, string documentId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _ddeiArgFactory.CreateDocumentArgDto(cmsAuthValues, correlationId, caseUrn, caseId, documentId);

        var ddeiClient = _ddeiClientFactory.Create(cmsAuthValues, DdeiClients.Mds);

        var result = await ddeiClient.GetDocumentNotesAsync(arg);

        return new OkObjectResult(result);
    }
}