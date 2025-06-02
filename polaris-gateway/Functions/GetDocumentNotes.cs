using Common.Configuration;
using Common.Extensions;
using Ddei.Factories;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetDocumentNotes : BaseFunction
{
    private readonly ILogger<GetDocumentNotes> _logger;
    private readonly IDdeiClientFactory _ddeiClientFactory;
    private readonly IDdeiArgFactory _ddeiArgFactory;

    public GetDocumentNotes(ILogger<GetDocumentNotes> logger,
        IDdeiClientFactory ddeiClientFactory,
        IDdeiArgFactory ddeiArgFactory)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
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