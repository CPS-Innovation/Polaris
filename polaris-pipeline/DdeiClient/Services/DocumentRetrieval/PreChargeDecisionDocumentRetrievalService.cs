using Common.Domain.Document;
using Common.Extensions;
using Common.Services.RenderHtmlService;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Factories;
using PolarisGateway.Models;

namespace DdeiClient.Services.DocumentRetrieval;

public class PreChargeDecisionDocumentRetrievalService : IDocumentRetrievalService
{
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IConvertModelToHtmlService _convertModelToHtmlService;
    private readonly IMdsClient _mdsClient;

    public PreChargeDecisionDocumentRetrievalService(IDdeiArgFactory ddeiArgFactory, IConvertModelToHtmlService convertModelToHtmlService, IMdsClient mdsClient)
    {
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _convertModelToHtmlService = convertModelToHtmlService.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    public async Task<DocumentRetrievalDto> GetDocumentAsync(string cmsAuthValues, Guid correlationId, string caseUrn, int caseId, string documentId,
        long versionId)
    {
        var ddeiPcdArgDto = _ddeiArgFactory.CreatePcdArg(cmsAuthValues, correlationId, caseUrn, caseId, documentId);
        var pcdRequest = await _mdsClient.GetPcdRequestAsync(ddeiPcdArgDto);

        var stream = await _convertModelToHtmlService.ConvertAsync(pcdRequest);
        return new DocumentRetrievalDto
        {
            Stream = stream,
            FileType = FileTypeHelper.PseudoDocumentFileType,
            IsKnownFileType = true
        };
    }
}