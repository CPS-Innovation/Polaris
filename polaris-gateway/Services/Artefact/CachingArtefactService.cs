using Common.Domain.Ocr;
using Common.Domain.Pii;
using Common.Services.BlobStorageService;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;
using static Common.Helpers.BlobNameHelper;

namespace PolarisGateway.Services.Artefact;
public class CachingArtefactService : ICachingArtefactService
{
    private readonly IArtefactServiceResponseFactory _artefactServiceResponseFactory;
    private readonly IPolarisBlobStorageService _blobStorageService;
    private readonly IArtefactService _artefactService;

    public CachingArtefactService(
        IArtefactServiceResponseFactory artefactServiceResponseFactory,
        IPolarisBlobStorageService blobStorageService,
        IArtefactService artefactService)
    {
        _artefactServiceResponseFactory = artefactServiceResponseFactory ?? throw new ArgumentNullException(nameof(artefactServiceResponseFactory));
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _artefactService = artefactService ?? throw new ArgumentNullException(nameof(artefactService));
    }

    public async Task<ArtefactResult<Stream>> GetPdf(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed)
    {
        var blobName = GetBlobName(caseId, documentId, versionId, BlobType.Pdf);
        var cachedBlobStream = await _blobStorageService.GetBlobAsync(blobName);
        if (cachedBlobStream != null)
        {
            return _artefactServiceResponseFactory.CreateOkfResult(cachedBlobStream, true);
        }

        var result = await _artefactService.GetPdf(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed);
        if (result.Status != ResultStatus.ArtefactAvailable)
        {
            return result;
        }

        await _blobStorageService.UploadBlobAsync(result.Result, blobName);
        var pdfStream = await _blobStorageService.GetBlobAsync(blobName);
        return _artefactServiceResponseFactory.CreateOkfResult(pdfStream, false);
    }

    public async Task<ArtefactResult<(Guid?, AnalyzeResults)>> GetOcr(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null)
    {
        var blobName = GetBlobName(caseId, documentId, versionId, BlobType.Ocr);
        var cachedResults = await _blobStorageService.GetObjectAsync<AnalyzeResults>(blobName);
        if (cachedResults != null)
        {
            return _artefactServiceResponseFactory.CreateOkfResult(((Guid?)null, cachedResults), true);
        }

        var result = await _artefactService.GetOcr(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed, operationId);
        if (result.Status != ResultStatus.ArtefactAvailable)
        {
            return result;
        }

        await _blobStorageService.UploadObjectAsync(result.Result.Item2, blobName);
        return _artefactServiceResponseFactory.CreateOkfResult(result.Result, false);
    }

    public async Task<ArtefactResult<(Guid?, IEnumerable<PiiLine>)>> GetPii(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null)
    {
        var blobName = GetBlobName(caseId, documentId, versionId, BlobType.Pii);
        var cachedResults = await _blobStorageService.GetObjectAsync<IEnumerable<PiiLine>>(blobName);
        if (cachedResults != null)
        {
            return _artefactServiceResponseFactory.CreateOkfResult(((Guid?)null, cachedResults), true);
        }

        var result = await _artefactService.GetPii(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed, operationId);
        if (result.Status != ResultStatus.ArtefactAvailable)
        {
            return result;
        }

        await _blobStorageService.UploadObjectAsync(result.Result.Item2, blobName);
        return _artefactServiceResponseFactory.CreateOkfResult(result.Result, false);
    }
}
