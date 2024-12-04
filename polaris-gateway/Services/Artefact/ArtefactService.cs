using Common.Clients.PdfGenerator;
using Common.Constants;
using Common.Domain.Document;
using Common.Domain.Ocr;
using Common.Domain.Pii;
using Common.Services.OcrService;
using Common.Services.PiiService;
using Common.Services.RenderHtmlService;
using Ddei;
using Ddei.Factories;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact;
public class ArtefactService : IArtefactService
{
    private readonly IArtefactServiceResponseFactory _artefactServiceResponseFactory;
    private readonly IDdeiClient _ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IPdfGeneratorClient _pdfGeneratorClient;
    private readonly IConvertModelToHtmlService _convertPcdRequestToHtmlService;
    private readonly IOcrService _ocrService;
    private readonly IPiiService _piiService;

    public ArtefactService(
        IArtefactServiceResponseFactory artefactServiceResponseFactory,
        IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        IPdfGeneratorClient pdfGeneratorClient,
        IConvertModelToHtmlService convertPcdRequestToHtmlService,
        IOcrService ocrService,
        IPiiService piiService)
    {
        _artefactServiceResponseFactory = artefactServiceResponseFactory ?? throw new ArgumentNullException(nameof(artefactServiceResponseFactory));
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _pdfGeneratorClient = pdfGeneratorClient ?? throw new ArgumentNullException(nameof(pdfGeneratorClient));
        _convertPcdRequestToHtmlService = convertPcdRequestToHtmlService ?? throw new ArgumentNullException(nameof(convertPcdRequestToHtmlService));
        _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        _piiService = piiService ?? throw new ArgumentNullException(nameof(piiService));
    }

    public async Task<ArtefactResult<Stream>> GetPdfAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed)
    {
        return await GetPdfInternalAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed);
    }

    public async Task<ArtefactResult<(Guid?, AnalyzeResults)>> GetOcrAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null)
    {
        Task<ArtefactResult<Stream>> pdfResult() => GetPdfAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed);

        return await GetOcrInternalAsync(pdfResult, correlationId, operationId);
    }

    public async Task<ArtefactResult<(Guid?, IEnumerable<PiiLine>)>> GetPiiAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null)
    {
        Task<ArtefactResult<(Guid?, AnalyzeResults)>> getOcr() => GetOcrAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed, operationId);

        return await GetPiiInternalAsync(getOcr, correlationId, caseId, documentId);
    }

    protected async Task<ArtefactResult<Stream>> GetPdfInternalAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed)
    {
        var prefix = documentId.Split('-')[0];
        var documentNature = DocumentNature.GetType(prefix);
        (PdfConversionStatus Status, Stream Stream) pdfResult = documentNature switch
        {
            DocumentNature.Types.PreChargeDecisionRequest => await GetPcdRequestStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId),
            DocumentNature.Types.DefendantsAndCharges => await GetDefendantsAndChargesStreamAsync(cmsAuthValues, correlationId, urn, caseId),
            _ => await GetDocumentStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId)
        };

        if (pdfResult.Status == PdfConversionStatus.DocumentConverted)
        {
            return _artefactServiceResponseFactory.CreateOkfResult(pdfResult.Stream, null);
        }
        return _artefactServiceResponseFactory.CreateFailedResult<Stream>(pdfResult.Status);
    }

    protected async Task<ArtefactResult<(Guid?, AnalyzeResults)>> GetOcrInternalAsync(Func<Task<ArtefactResult<Stream>>> getPdf, Guid correlationId, Guid? operationId = null)
    {
        if (operationId.HasValue)
        {
            var ocrResult = await _ocrService.GetOperationResultsAsync(operationId.Value, correlationId);
            if (ocrResult.IsSuccess)
            {
                return _artefactServiceResponseFactory.CreateOkfResult<(Guid?, AnalyzeResults)>((null, ocrResult.AnalyzeResults), false);
            }

            return _artefactServiceResponseFactory.CreateInterimResult<(Guid?, AnalyzeResults)>((operationId, null));
        }

        var pdfResult = await getPdf();
        if (pdfResult.Status == ResultStatus.ArtefactAvailable)
        {
            var newOperationId = await _ocrService.InitiateOperationAsync(pdfResult.Result, correlationId);
            return _artefactServiceResponseFactory.CreateInterimResult<(Guid?, AnalyzeResults)>((newOperationId, null));
        }

        return _artefactServiceResponseFactory.CreateFailedResult<(Guid?, AnalyzeResults)>(pdfResult.PdfConversionStatus);
    }

    protected async Task<ArtefactResult<(Guid?, IEnumerable<PiiLine>)>> GetPiiInternalAsync(Func<Task<ArtefactResult<(Guid?, AnalyzeResults)>>> getOcr, Guid correlationId, int caseId, string documentId)
    {
        var ocrResult = await getOcr();
        if (ocrResult.Status == ResultStatus.ArtefactAvailable)
        {
            var piiResult = await _piiService.GetPiiResultsAsync(ocrResult.Result.Item2, caseId, documentId, correlationId);
            return _artefactServiceResponseFactory.CreateOkfResult(((Guid?)null, piiResult), null);
        }
        if (ocrResult.Status == ResultStatus.PollWithToken)
        {
            return _artefactServiceResponseFactory.CreateInterimResult((ocrResult.Result.Item1, (IEnumerable<PiiLine>)null));
        }

        return _artefactServiceResponseFactory.CreateFailedResult<(Guid?, IEnumerable<PiiLine>)>(ocrResult.PdfConversionStatus);
    }

    private async Task<(PdfConversionStatus, Stream)> GetDocumentStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId)
    {
        var ddeiArgs = _ddeiArgFactory.CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);

        var fileResult = await _ddeiClient.GetDocumentAsync(ddeiArgs);
        if (!FileTypeHelper.TryGetSupportedFileType(fileResult.FileName, out var fileType))
        {
            return (PdfConversionStatus.DocumentTypeUnsupported, null);
        }

        var pdfResult = await _pdfGeneratorClient.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, fileResult.Stream, fileType);
        return (pdfResult.Status, pdfResult.PdfStream);
    }

    private async Task<(PdfConversionStatus, Stream)> GetPcdRequestStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId)
    {
        var arg = _ddeiArgFactory.CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, documentId);
        var pcdRequest = await _ddeiClient.GetPcdRequestAsync(arg);
        return (PdfConversionStatus.DocumentConverted, await _convertPcdRequestToHtmlService.ConvertAsync(pcdRequest));
    }

    private async Task<(PdfConversionStatus, Stream)> GetDefendantsAndChargesStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId)
    {
        var arg = _ddeiArgFactory.CreateCaseIdentifiersArg(
                                    cmsAuthValues,
                                    correlationId,
                                    urn,
                                    caseId);
        var defendantsAndCharges = await _ddeiClient.GetDefendantAndChargesAsync(arg);
        return (PdfConversionStatus.DocumentConverted, await _convertPcdRequestToHtmlService.ConvertAsync(defendantsAndCharges));
    }
}
