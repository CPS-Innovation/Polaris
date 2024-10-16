using System.Text;
using System.Text.RegularExpressions;
using Common.Clients.PdfGenerator;
using Common.Constants;
using Common.Domain.Document;
using Common.Helpers;
using Common.Services.BlobStorageService;
using Common.Services.OcrService;
using Common.Wrappers;
using Ddei;
using Ddei.Factories;
using PolarisGateway.Services.Domain;

namespace PolarisGateway.Services;

public class ArtefactService : IArtefactService
{
    private readonly IArtefactServiceResponseFactory _artefactServiceResponseFactory;
    private readonly IV2PolarisBlobStorageService _blobStorageService;
    private readonly IDdeiClient _ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IPdfGeneratorClient _pdfGeneratorClient;
    private readonly IOcrService _ocrService;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;

    public ArtefactService(
        IArtefactServiceResponseFactory artefactServiceResponseFactory,
        IV2PolarisBlobStorageService blobStorageService,
        IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        IPdfGeneratorClient pdfGeneratorClient,
        IOcrService ocrService,
        IJsonConvertWrapper jsonConvertWrapper)
    {
        _artefactServiceResponseFactory = artefactServiceResponseFactory ?? throw new ArgumentNullException(nameof(artefactServiceResponseFactory));
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _pdfGeneratorClient = pdfGeneratorClient ?? throw new ArgumentNullException(nameof(pdfGeneratorClient));
        _ocrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
    }

    public async Task<PdfResult> GetPdf(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed)
    {
        var blobName = BlobNameHelper.GetBlobName(caseId, documentId, versionId, BlobNameHelper.BlobType.Pdf);

        var metaData = new Dictionary<string, string> { { "isOcrProcessed", isOcrProcessed.ToString() } };

        var blobStream = await _blobStorageService.GetDocumentAsync(blobName, mustMatchMetadata: metaData);
        if (blobStream != null)
        {
            return _artefactServiceResponseFactory.CreateOkGetPdfResult(blobStream, true);
        }

        var documentIdWithoutPrefix = long.Parse(Regex.Match(documentId, @"\d+").Value);
        var ddeiArgs = _ddeiArgFactory.CreateDocumentArgDto(cmsAuthValues, correlationId, urn, caseId, documentIdWithoutPrefix, versionId);

        var fileResult = await _ddeiClient.GetDocumentAsync(ddeiArgs);
        var fileExtension = Path.GetExtension(fileResult.FileName)
            .Replace(".", string.Empty)
            .ToUpperInvariant();

        var isRecognisedFileType = Enum.TryParse<FileType>(fileExtension, out var fileType);
        if (!isRecognisedFileType)
        {
            return _artefactServiceResponseFactory.CreateFailedGetPdfResult(PdfConversionStatus.DocumentTypeUnsupported);
        }

        var pdfResult = await _pdfGeneratorClient.ConvertToPdfAsync(correlationId, urn, caseId, documentId, versionId, fileResult.Stream, fileType);
        if (pdfResult.Status != PdfConversionStatus.DocumentConverted)
        {
            return _artefactServiceResponseFactory.CreateFailedGetPdfResult(pdfResult.Status);
        }

        await _blobStorageService.UploadDocumentAsync(pdfResult.PdfStream, blobName, metaData);
        var pdfStream = await _blobStorageService.GetDocumentAsync(blobName, metaData);

        return _artefactServiceResponseFactory.CreateOkGetPdfResult(pdfStream, false);
    }

    public async Task<JsonArtefactResult> GetOcr(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null)
    {
        var blobName = BlobNameHelper.GetBlobName(caseId, documentId, versionId, BlobNameHelper.BlobType.Ocr);

        if (operationId.HasValue)
        {
            var ocrResult = await _ocrService.GetOperationResultsAsync(operationId.Value, correlationId);
            if (ocrResult.IsSuccess)
            {
                var ocrResultString = _jsonConvertWrapper.SerializeObject(ocrResult.AnalyzeResults);
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(ocrResultString ?? ""));

                await _blobStorageService.UploadDocumentAsync(stream, blobName);
                var resultStream = await _blobStorageService.GetDocumentAsync(blobName);

                return _artefactServiceResponseFactory.CreateOcrResultsAvailableOcrResult(resultStream, false);
            }

            return _artefactServiceResponseFactory.CreatePollWithTokenInitiateOcrResult(operationId.Value);
        }

        var blobStream = await _blobStorageService.GetDocumentAsync(blobName);
        if (blobStream != null)
        {
            return _artefactServiceResponseFactory.CreateOcrResultsAvailableOcrResult(blobStream, true);
        }

        var pdfResult = await GetPdf(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed);
        if (pdfResult.Status == PdfResult.ResultStatus.PdfAvailable)
        {
            var newOperationId = await _ocrService.InitiateOperationAsync(pdfResult.Stream, correlationId);
            return _artefactServiceResponseFactory.CreatePollWithTokenInitiateOcrResult(newOperationId);
        }

        return _artefactServiceResponseFactory.CreateFailedOnPdfConversionOcrResult(pdfResult.PdfConversionStatus);
    }

    public async Task<JsonArtefactResult> GetPII(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null)
    {
        var blobName = BlobNameHelper.GetBlobName(caseId, documentId, versionId, BlobNameHelper.BlobType.Pii);

        Stream ocrStream = null;
        if (operationId.HasValue)
        {
            var ocrResult = await GetOcr(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed, operationId);
            if (ocrResult.Status != JsonArtefactResult.ResultStatus.ArtefactAvailable)
            {
                return ocrResult;
            }
            ocrStream = ocrResult.Stream;
        }
        else
        {
            var blobStream = await _blobStorageService.GetDocumentAsync(blobName);
            if (blobStream != null)
            {
                return _artefactServiceResponseFactory.CreateOcrResultsAvailableOcrResult(blobStream, true);
            }
        }




    }
}
