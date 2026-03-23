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

    public async Task<DocumentRetrievalResult> GetPdfStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, System.Threading.CancellationToken cancellationToken = default)
    {
        var (stream, fileType, isKnownFileType) = DocumentNature.GetDocumentNatureType(documentId) switch
        {
            DocumentNature.Types.PreChargeDecisionRequest => await GetPcdRequestStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, cancellationToken),
            DocumentNature.Types.DefendantsAndCharges => await GetDefendantsAndChargesStreamAsync(cmsAuthValues, correlationId, urn, caseId, cancellationToken),
            _ => await GetDocumentStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, cancellationToken)
        };

        if (!isKnownFileType)
        {
            return new DocumentRetrievalResult
            {
                Status = PdfConversionStatus.DocumentTypeUnsupported,
            };
        }

        var pdfResult = await _pdfGeneratorClient.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, stream, fileType, cancellationToken);

        return new DocumentRetrievalResult
        {
            PdfStream = pdfResult.Status == PdfConversionStatus.DocumentConverted ? pdfResult.PdfStream : null,
            Status = pdfResult.Status,
        };
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDocumentStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId)
    {
        return await GetDocumentStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, System.Threading.CancellationToken.None);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDocumentStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, System.Threading.CancellationToken cancellationToken)
    {
        var mdsDocumentIdAndVersionIdArgDto = _mdsArgFactory.CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);
        var fileResult = await _mdsClient.GetDocumentAsync(mdsDocumentIdAndVersionIdArgDto, cancellationToken);

        var isKnownFileType = FileTypeHelper.TryGetSupportedFileType(fileResult.FileName, out var fileType);
        return (fileResult.Stream, fileType, isKnownFileType);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetPcdRequestStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId)
        => await GetPcdRequestStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, System.Threading.CancellationToken.None);

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetPcdRequestStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, System.Threading.CancellationToken cancellationToken)
    {
        var mdsPcdArgDto = _mdsArgFactory.CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, documentId);
        var pcdRequest = await _mdsClient.GetPcdRequestAsync(mdsPcdArgDto, cancellationToken);

        var stream = await _convertModelToHtmlService.ConvertAsync(pcdRequest);
        return (stream, FileTypeHelper.PseudoDocumentFileType, true);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDefendantsAndChargesStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId)
        => await GetDefendantsAndChargesStreamAsync(cmsAuthValues, correlationId, urn, caseId, System.Threading.CancellationToken.None);

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDefendantsAndChargesStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, System.Threading.CancellationToken cancellationToken)
    {
        var mdsCaseIdentifiersArgDto = _mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, urn, caseId);
        var defendantsAndCharges = await _mdsClient.GetDefendantAndChargesAsync(mdsCaseIdentifiersArgDto, cancellationToken);
        var stream = await _convertModelToHtmlService.ConvertAsync(defendantsAndCharges);
        return (stream, FileTypeHelper.PseudoDocumentFileType, true);
    }
}