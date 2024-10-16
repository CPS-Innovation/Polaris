using System.Text.RegularExpressions;
using Common.Clients.PdfGenerator;
using Common.Constants;
using Common.Domain.Document;
using Common.Domain.Ocr;
using Common.Domain.Pii;
using Common.Services.OcrService;
using Common.Services.PiiService;
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
    private readonly IOcrService _ocrService;
    private readonly IPiiService _piiService;

    public ArtefactService(
        IArtefactServiceResponseFactory artefactServiceResponseFactory,
        IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        IPdfGeneratorClient pdfGeneratorClient,
        IOcrService ocrService,
        IPiiService piiService)
    {
        _artefactServiceResponseFactory = artefactServiceResponseFactory ?? throw new ArgumentNullException(nameof(artefactServiceResponseFactory));
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _pdfGeneratorClient = pdfGeneratorClient ?? throw new ArgumentNullException(nameof(pdfGeneratorClient));
        _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        _piiService = piiService ?? throw new ArgumentNullException(nameof(piiService));
    }

    public async Task<ArtefactResult<Stream>> GetPdf(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed)
    {
        var documentIdWithoutPrefix = long.Parse(Regex.Match(documentId, @"\d+").Value);
        var ddeiArgs = _ddeiArgFactory.CreateDocumentArgDto(cmsAuthValues, correlationId, urn, caseId, documentIdWithoutPrefix, versionId);
        var fileResult = await _ddeiClient.GetDocumentAsync(ddeiArgs);

        if (!FiletypeHelper.TryGetSupportedFileType(fileResult.FileName, out var fileType))
        {
            return _artefactServiceResponseFactory.CreateFailedResult<Stream>(PdfConversionStatus.DocumentTypeUnsupported);
        }

        var pdfResult = await _pdfGeneratorClient.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, fileResult.Stream, fileType);
        if (pdfResult.Status == PdfConversionStatus.DocumentConverted)
        {
            return _artefactServiceResponseFactory.CreateOkfResult(pdfResult.PdfStream, null);
        }

        return _artefactServiceResponseFactory.CreateFailedResult<Stream>(pdfResult.Status);
    }

    public async Task<ArtefactResult<(Guid?, AnalyzeResults)>> GetOcr(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null)
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

        var pdfResult = await GetPdf(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed);
        if (pdfResult.Status == ResultStatus.ArtefactAvailable)
        {
            var newOperationId = await _ocrService.InitiateOperationAsync(pdfResult.Result, correlationId);
            return _artefactServiceResponseFactory.CreateInterimResult<(Guid?, AnalyzeResults)>((newOperationId, null));
        }

        return _artefactServiceResponseFactory.CreateFailedResult<(Guid?, AnalyzeResults)>(pdfResult.PdfConversionStatus);
    }

    public async Task<ArtefactResult<(Guid?, IEnumerable<PiiLine>)>> GetPii(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null)
    {
        var ocrResult = await GetOcr(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed, operationId);
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
}
