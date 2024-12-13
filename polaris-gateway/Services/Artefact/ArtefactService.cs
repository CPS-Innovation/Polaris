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

namespace PolarisGateway.Services.Artefact;
public class ArtefactService : IArtefactService
{
    private readonly IArtefactServiceResponseFactory _artefactServiceResponseFactory;
    private readonly IDdeiClient _ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IPdfGeneratorClient _pdfGeneratorClient;
    private readonly IConvertModelToHtmlService _convertModelToHtmlService;
    private readonly IOcrService _ocrService;
    private readonly IPiiService _piiService;

    public ArtefactService(
        IArtefactServiceResponseFactory artefactServiceResponseFactory,
        IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        IPdfGeneratorClient pdfGeneratorClient,
        IConvertModelToHtmlService convertModelToHtmlService,
        IOcrService ocrService,
        IPiiService piiService)
    {
        _artefactServiceResponseFactory = artefactServiceResponseFactory ?? throw new ArgumentNullException(nameof(artefactServiceResponseFactory));
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _pdfGeneratorClient = pdfGeneratorClient ?? throw new ArgumentNullException(nameof(pdfGeneratorClient));
        _convertModelToHtmlService = convertModelToHtmlService ?? throw new ArgumentNullException(nameof(convertModelToHtmlService));
        _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        _piiService = piiService ?? throw new ArgumentNullException(nameof(piiService));
    }

    public async Task<ArtefactResult<Stream>> GetPdfAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed)
    {
        return await GetPdfInternalAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);
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

    protected async Task<ArtefactResult<Stream>> GetPdfInternalAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId)
    {
        var (stream, fileType, isKnownFileType) = DocumentNature.GetDocumentNatureType(documentId) switch
        {
            DocumentNature.Types.PreChargeDecisionRequest => await GetPcdRequestStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId),
            DocumentNature.Types.DefendantsAndCharges => await GetDefendantsAndChargesStreamAsync(cmsAuthValues, correlationId, urn, caseId),
            _ => await GetDocumentStreamAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId)
        };

        if (!isKnownFileType)
        {
            return _artefactServiceResponseFactory.CreateFailedResult<Stream>(PdfConversionStatus.DocumentTypeUnsupported);
        }

        var pdfResult = await _pdfGeneratorClient.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, stream, fileType);

        return pdfResult.Status == PdfConversionStatus.DocumentConverted
            ? _artefactServiceResponseFactory.CreateOkfResult(pdfResult.PdfStream, null)
            : _artefactServiceResponseFactory.CreateFailedResult<Stream>(pdfResult.Status);
    }

    protected async Task<ArtefactResult<(Guid?, AnalyzeResults)>> GetOcrInternalAsync(Func<Task<ArtefactResult<Stream>>> getPdf, Guid correlationId, Guid? operationId = null)
    {
        if (operationId.HasValue)
        {
            var ocrResult = await _ocrService.GetOperationResultsAsync(operationId.Value, correlationId);
            return ocrResult.IsSuccess
                ? _artefactServiceResponseFactory.CreateOkfResult<(Guid?, AnalyzeResults)>((null, ocrResult.AnalyzeResults), false)
                : _artefactServiceResponseFactory.CreateInterimResult<(Guid?, AnalyzeResults)>((operationId, null));
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

        return ocrResult.Status == ResultStatus.PollWithToken
            ? _artefactServiceResponseFactory.CreateInterimResult((ocrResult.Result.Item1, (IEnumerable<PiiLine>)null))
            : _artefactServiceResponseFactory.CreateFailedResult<(Guid?, IEnumerable<PiiLine>)>(ocrResult.PdfConversionStatus);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDocumentStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId)
    {
        var ddeiArgs = _ddeiArgFactory.CreateDocumentVersionArgDto(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);
        var fileResult = await _ddeiClient.GetDocumentAsync(ddeiArgs);

        var isKnownFileType = FileTypeHelper.TryGetSupportedFileType(fileResult.FileName, out var fileType);
        return (fileResult.Stream, fileType, isKnownFileType);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetPcdRequestStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId)
    {
        var arg = _ddeiArgFactory.CreatePcdArg(cmsAuthValues, correlationId, urn, caseId, documentId);
        var pcdRequest = await _ddeiClient.GetPcdRequestAsync(arg);

        var stream = await _convertModelToHtmlService.ConvertAsync(pcdRequest);
        return (stream, FileTypeHelper.PseudoDocumentFileType, true);
    }

    private async Task<(Stream Stream, FileType FileType, bool IsKnownFileType)> GetDefendantsAndChargesStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId)
    {
        var arg = _ddeiArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, urn, caseId);
        var defendantsAndCharges = await _ddeiClient.GetDefendantAndChargesAsync(arg);
        var stream = await _convertModelToHtmlService.ConvertAsync(defendantsAndCharges);
        return (stream, FileTypeHelper.PseudoDocumentFileType, true);
    }
}
