using Common.Clients.PdfGenerator;
using Common.Constants;
using Common.Domain.Document;
using Common.Extensions;
using Common.Services.RenderHtmlService;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using PolarisGateway.Services.Artefact.Domain;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact;

public class PdfRetrievalService : IPdfRetrievalService
{
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IConvertModelToHtmlService _convertModelToHtmlService;
    private readonly IPdfGeneratorClient _pdfGeneratorClient;
    private readonly IMdsClient _mdsClient;
    private readonly IDdeiAuthClient _ddeiAuthClient;

    public PdfRetrievalService(
        IDdeiArgFactory ddeiArgFactory,
        IConvertModelToHtmlService convertModelToHtmlService,
        IPdfGeneratorClient pdfGeneratorClient,
        IMdsClient mdsClient,
        IDdeiAuthClient ddeiAuthClient)
    {
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _convertModelToHtmlService = convertModelToHtmlService.ExceptionIfNull();
        _pdfGeneratorClient = pdfGeneratorClient.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
        _ddeiAuthClient = ddeiAuthClient.ExceptionIfNull();
    }

    public async Task<DocumentRetrievalResult> GetPdfStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId)
    {
        var (stream, fileType, isKnownFileType) = DocumentNature.GetDocumentNatureType(documentId) switch
        {
            DocumentNature.Types.PreChargeDecisionRequest => await GetPcdRequestStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId),
            DocumentNature.Types.DefendantsAndCharges => await GetDefendantsAndChargesStreamAsync(cmsAuthValues, correlationId, urn, caseId),
            _ => await GetDocumentStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId)
        };

        if (!isKnownFileType)
        {
            return new DocumentRetrievalResult
            {
                Status = PdfConversionStatus.DocumentTypeUnsupported,
            };
        }

        var pdfResult = await _pdfGeneratorClient.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, stream, fileType);

        return new DocumentRetrievalResult
        {
            PdfStream = pdfResult.Status == PdfConversionStatus.DocumentConverted ? pdfResult.PdfStream : null,
            Status = pdfResult.Status,
        };
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDocumentStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId)
    {
        var ddeiDocumentIdAndVersionIdArgDto = _ddeiArgFactory.CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);
        var fileResult = await _ddeiAuthClient.GetDocumentAsync(ddeiDocumentIdAndVersionIdArgDto);

        var isKnownFileType = FileTypeHelper.TryGetSupportedFileType(fileResult.FileName, out var fileType);
        return (fileResult.Stream, fileType, isKnownFileType);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetPcdRequestStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId)
    {
        var ddeiPcdArgDto = _ddeiArgFactory.CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, documentId);
        var pcdRequest = await _mdsClient.GetPcdRequestAsync(ddeiPcdArgDto);

        var stream = await _convertModelToHtmlService.ConvertAsync(pcdRequest);
        return (stream, FileTypeHelper.PseudoDocumentFileType, true);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDefendantsAndChargesStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId)
    {
        var deiCaseIdentifiersArgDto = _ddeiArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, urn, caseId);
        var defendantsAndCharges = await _mdsClient.GetDefendantAndChargesAsync(deiCaseIdentifiersArgDto);
        var stream = await _convertModelToHtmlService.ConvertAsync(defendantsAndCharges);
        return (stream, FileTypeHelper.PseudoDocumentFileType, true);
    }
}