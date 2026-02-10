using Common.Configuration;
using Common.Domain.Document;
using Common.Extensions;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Domain.Args;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class ToggleIsUnusedDocument : BaseFunction
{
    private readonly ILogger<ToggleIsUnusedDocument> _logger;
    private readonly IMdsClient _mdsClient;
    public ToggleIsUnusedDocument(
        ILogger<ToggleIsUnusedDocument> logger,
        IMdsClient mdsClient)
    {
        _logger = logger.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    [Function(nameof(ToggleIsUnusedDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ToggleIsUnusedDocument)] HttpRequest req,
        string caseUrn,
        int caseId,
        string documentId,
        string isUnused)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var toggleIsUnusedDocumentDto = new MdsToggleIsUnusedDocumentDto
        {
            CaseId = caseId,
            CmsAuthValues = cmsAuthValues,
            CorrelationId = correlationId,
            DocumentId = DocumentNature.ToNumericDocumentId(documentId, DocumentNature.Types.Document),
            IsUnused = isUnused,
            Urn = caseUrn,
        };

        return await _mdsClient.ToggleIsUnusedDocumentAsync(toggleIsUnusedDocumentDto) ? new OkResult() : new BadRequestResult();
    }
}