using Common.Domain.Document;
using Common.Extensions;
using Common.Services.RenderHtmlService;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Factories;
using PolarisGateway.Models;

namespace DdeiClient.Services.DocumentRetrieval;

public class DefendantsAndChargesDocumentRetrievalService : IDocumentRetrievalService
{
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IConvertModelToHtmlService _convertModelToHtmlService;
    private readonly IMdsClient _mdsClient;

    public DefendantsAndChargesDocumentRetrievalService(IDdeiArgFactory ddeiArgFactory, IConvertModelToHtmlService convertModelToHtmlService, IMdsClient mdsClient)
    {
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _convertModelToHtmlService = convertModelToHtmlService.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    public async Task<DocumentRetrievalDto> GetDocumentAsync(string cmsAuthValues, Guid correlationId, string caseUrn, int caseId, string documentId, long versionId)
    {
        var deiCaseIdentifiersArgDto = _ddeiArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);
        var defendantsAndCharges = await _mdsClient.GetDefendantAndChargesAsync(deiCaseIdentifiersArgDto);
        var stream = await _convertModelToHtmlService.ConvertAsync(defendantsAndCharges);
        return new DocumentRetrievalDto
        {
            Stream = stream,
            FileType = FileTypeHelper.PseudoDocumentFileType,
            IsKnownFileType = true
        };
    }
}