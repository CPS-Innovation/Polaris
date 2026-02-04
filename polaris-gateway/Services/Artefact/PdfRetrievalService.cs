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
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly IConvertModelToHtmlService _convertModelToHtmlService;
    private readonly IPdfGeneratorClient _pdfGeneratorClient;
    private readonly IMdsClient _mdsClient;

    public PdfRetrievalService(
        IMdsArgFactory mdsArgFactory,
        IConvertModelToHtmlService convertModelToHtmlService,
        IPdfGeneratorClient pdfGeneratorClient, 
        IMdsClient mdsClient)
    {
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _convertModelToHtmlService = convertModelToHtmlService.ExceptionIfNull();
        _pdfGeneratorClient = pdfGeneratorClient.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
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
        var mdsDocumentIdAndVersionIdArgDto = _mdsArgFactory.CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);
        var fileResult = await _mdsClient.GetDocumentAsync(mdsDocumentIdAndVersionIdArgDto);

        var isKnownFileType = FileTypeHelper.TryGetSupportedFileType(fileResult.FileName, out var fileType);
        return (fileResult.Stream, fileType, isKnownFileType);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetPcdRequestStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId)
    {
        var mdsPcdArgDto = _mdsArgFactory.CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, documentId);
        var pcdRequest = await _mdsClient.GetPcdRequestAsync(mdsPcdArgDto);

        var stream = await _convertModelToHtmlService.ConvertAsync(pcdRequest);
        return (stream, FileTypeHelper.PseudoDocumentFileType, true);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDefendantsAndChargesStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId)
    {
        var mdsCaseIdentifiersArgDto = _mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, urn, caseId);
        var defendantsAndCharges = await _mdsClient.GetDefendantAndChargesAsync(mdsCaseIdentifiersArgDto);
        var stream = await _convertModelToHtmlService.ConvertAsync(defendantsAndCharges);
        return (stream, FileTypeHelper.PseudoDocumentFileType, true);
    }
}