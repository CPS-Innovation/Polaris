using Common.Configuration;
using Common.Extensions;
using DdeiClient.Domain.Args;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class ToggleIsUnusedDocument : BaseFunction
{
    private readonly ILogger<ToggleIsUnusedDocument> _logger;
    private readonly IDdeiClientFactory _ddeiClientFactory;
    public ToggleIsUnusedDocument(
        ILogger<ToggleIsUnusedDocument> logger,
        IDdeiClientFactory ddeiClientFactory)
    {
        _logger = logger.ExceptionIfNull();
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
    }

    [Function(nameof(ToggleIsUnusedDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ToggleIsUnusedDocument)] HttpRequest req,
        string caseUrn,
        int caseId,
        long documentId,
        string isUnused)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var toggleIsUnusedDocumentDto = new DdeiToggleIsUnusedDocumentDto
        {
            CaseId = caseId,
            CmsAuthValues = cmsAuthValues,
            CorrelationId = correlationId,
            DocumentId = documentId,
            IsUnused = isUnused,
            Urn = caseUrn,
        };

        var ddeiClient = _ddeiClientFactory.Create(cmsAuthValues, DdeiClients.Mds);

        return await ddeiClient.ToggleIsUnusedDocumentAsync(toggleIsUnusedDocumentDto) ? new OkResult() : new BadRequestResult();
    }
}