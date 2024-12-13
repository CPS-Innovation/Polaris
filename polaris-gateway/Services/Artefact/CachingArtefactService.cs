using Common.Clients.PdfGenerator;
using Common.Configuration;
using Common.Domain.Ocr;
using Common.Domain.Pii;
using Common.Services.BlobStorage;
using Common.Services.OcrService;
using Common.Services.PiiService;
using Common.Services.RenderHtmlService;
using Ddei;
using Ddei.Factories;
using Microsoft.Extensions.Configuration;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;

namespace PolarisGateway.Services.Artefact;
public class CachingArtefactService : ArtefactService, ICachingArtefactService
{
    private readonly IArtefactServiceResponseFactory _artefactServiceResponseFactory;
    private readonly IPolarisBlobStorageService _polarisBlobStorageService;


    public CachingArtefactService(
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IArtefactServiceResponseFactory artefactServiceResponseFactory,
        IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        IPdfGeneratorClient pdfGeneratorClient,
        IConvertModelToHtmlService convertPcdRequestToHtmlService,
        IOcrService ocrService,
        IPiiService piiService,
        IConfiguration configuration
        ) : base(artefactServiceResponseFactory, ddeiClient, ddeiArgFactory, pdfGeneratorClient, convertPcdRequestToHtmlService, ocrService, piiService)
    {
        _artefactServiceResponseFactory = artefactServiceResponseFactory ?? throw new ArgumentNullException(nameof(artefactServiceResponseFactory));
        _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
    }

    public new async Task<ArtefactResult<Stream>> GetPdfAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed)
    {
        var blobId = new BlobIdType(caseId, documentId, versionId, BlobType.Pdf);
        var cachedBlobStream = await _polarisBlobStorageService.TryGetBlobAsync(blobId, isOcrProcessed);
        if (cachedBlobStream != null)
        {
            return _artefactServiceResponseFactory.CreateOkfResult(cachedBlobStream, true);
        }

        var result = await GetPdfInternalAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId);

        if (result.Status != ResultStatus.ArtefactAvailable)
        {
            return result;
        }

        await _polarisBlobStorageService.UploadBlobAsync(result.Result, blobId, isOcrProcessed);
        var pdfStream = await _polarisBlobStorageService.TryGetBlobAsync(blobId);
        return _artefactServiceResponseFactory.CreateOkfResult(pdfStream, false);
    }

    public new async Task<ArtefactResult<(Guid?, AnalyzeResults)>> GetOcrAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null)
    {
        var blobId = new BlobIdType(caseId, documentId, versionId, BlobType.Ocr);
        var cachedResults = await _polarisBlobStorageService.TryGetObjectAsync<AnalyzeResults>(blobId);
        if (cachedResults != null)
        {
            return _artefactServiceResponseFactory.CreateOkfResult(((Guid?)null, cachedResults), true);
        }

        Task<ArtefactResult<Stream>> getPdfAsync() => GetPdfAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed);

        var result = await GetOcrInternalAsync(getPdfAsync, correlationId, operationId);
        if (result.Status != ResultStatus.ArtefactAvailable)
        {
            return result;
        }

        await _polarisBlobStorageService.UploadObjectAsync(result.Result.Item2, blobId);
        return _artefactServiceResponseFactory.CreateOkfResult(result.Result, false);
    }

    public new async Task<ArtefactResult<(Guid?, IEnumerable<PiiLine>)>> GetPiiAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null)
    {
        var blobId = new BlobIdType(caseId, documentId, versionId, BlobType.Pii);
        var cachedResults = await _polarisBlobStorageService.TryGetObjectAsync<IEnumerable<PiiLine>>(blobId);
        if (cachedResults != null)
        {
            return _artefactServiceResponseFactory.CreateOkfResult(((Guid?)null, cachedResults), true);
        }

        Task<ArtefactResult<(Guid?, AnalyzeResults)>> getOcrAsync() => GetOcrAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed, operationId);

        var result = await GetPiiInternalAsync(getOcrAsync, correlationId, caseId, documentId);
        if (result.Status != ResultStatus.ArtefactAvailable)
        {
            return result;
        }

        await _polarisBlobStorageService.UploadObjectAsync(result.Result.Item2, blobId);
        return _artefactServiceResponseFactory.CreateOkfResult(result.Result, false);
    }
}
