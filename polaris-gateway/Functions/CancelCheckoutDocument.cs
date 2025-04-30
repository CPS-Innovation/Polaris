using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ddei.Factories;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using System;
using Common.Extensions;
using Common.Telemetry;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;
using Microsoft.Extensions.DependencyInjection;
using Ddei.Domain.CaseData.Args;
using DdeiClient.Factories;

namespace PolarisGateway.Functions;

public class CancelCheckoutDocument : BaseFunction
{
    private readonly ILogger<CancelCheckoutDocument> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IDdeiClientFactory _ddeiClientFactory;

    public CancelCheckoutDocument(
        ILogger<CancelCheckoutDocument> logger,
        IDdeiArgFactory ddeiArgFactory,
        IDdeiClientFactory ddeiClientFactory)
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
    }

    [Function(nameof(CancelCheckoutDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.DocumentCheckout)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var ddeiDocumentIdAndVersionIdArgDto = _ddeiArgFactory.CreateDocumentVersionArgDto(
                cmsAuthValues: cmsAuthValues,
                correlationId: correlationId,
                urn: caseUrn,
                caseId: caseId,
                documentId: documentId,
                versionId: versionId);

        var ddeiClient = _ddeiClientFactory.Create(cmsAuthValues, DdeiClients.Mds);

        await ddeiClient.CancelCheckoutDocumentAsync(ddeiDocumentIdAndVersionIdArgDto);

        return new OkResult();
    }
}