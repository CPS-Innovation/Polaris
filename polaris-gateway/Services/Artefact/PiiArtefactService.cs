using Common.Domain.Ocr;
using Common.Domain.Pii;
using Common.Services.BlobStorage;
using Common.Services.PiiService;
using PolarisGateway.Services.Artefact.Domain;
using PolarisGateway.Services.Artefact.Factories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Extensions;

namespace PolarisGateway.Services.Artefact;

public class PiiArtefactService : IPiiArtefactService
{
    private readonly IArtefactServiceResponseFactory _artefactServiceResponseFactory;
    private readonly ICacheService _cacheService;
    private readonly IPiiService _piiService;
    private readonly IOcrArtefactService _ocrArtefactService;

    public PiiArtefactService(
        ICacheService cacheService,
        IArtefactServiceResponseFactory artefactServiceResponseFactory,
        IPiiService piiService,
        IOcrArtefactService ocrArtefactService
        )
    {
        _cacheService = cacheService.ExceptionIfNull();
        _artefactServiceResponseFactory = artefactServiceResponseFactory.ExceptionIfNull();
        _piiService = piiService.ExceptionIfNull();
        _ocrArtefactService = ocrArtefactService.ExceptionIfNull();
    }

    public async Task<ArtefactResult<IEnumerable<PiiLine>>> GetPiiAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null, bool forceRefresh = false)
    {
        if (!forceRefresh && await _cacheService.TryGetJsonObjectAsync<IEnumerable<PiiLine>>(caseId, documentId, versionId, BlobType.Pii) is (true, var results))
        {
            return _artefactServiceResponseFactory.CreateOkfResult(results, true);
        }

        var ocrResult = await _ocrArtefactService.GetOcrAsync(cmsAuthValues, correlationId, urn, caseId, documentId, versionId, isOcrProcessed, operationId, forceRefresh);

        if (ocrResult.Status != ResultStatus.ArtefactAvailable)
        {
            return _artefactServiceResponseFactory.ConvertNonOkResult<AnalyzeResults, IEnumerable<PiiLine>>(ocrResult);
        }

        var piiResult = await _piiService.GetPiiResultsAsync(ocrResult.Artefact, correlationId);
        await _cacheService.UploadJsonObjectAsync(caseId, documentId, versionId, BlobType.Pii, piiResult);
        return _artefactServiceResponseFactory.CreateOkfResult(piiResult, false);
    }
}
