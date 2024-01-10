using Microsoft.Extensions.Logging;
using polaris_common.Constants;
using polaris_common.Domain.Extensions;
using polaris_common.Dto.Request;
using polaris_common.Dto.Response;
using polaris_common.Logging;
using polaris_common.Services.BlobStorageService.Contracts;
using polaris_common.Services.DocumentEvaluation.Contracts;

namespace polaris_common.Services.DocumentEvaluation;

public class DocumentEvaluationService : IDocumentEvaluationService
{
    private readonly IPolarisBlobStorageService _blobStorageService;
    private readonly ILogger<DocumentEvaluationService> _logger;
    
    public DocumentEvaluationService(IPolarisBlobStorageService blobStorageService, ILogger<DocumentEvaluationService> logger)
    {
        _blobStorageService = blobStorageService;
        _logger = logger;
    }
    
    /// <summary>
    /// Attempt to find the incoming CMS document's details in the documents already stored for the case
    /// </summary>
    /// <param name="request"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    public async Task<EvaluateDocumentResponse> EvaluateDocumentAsync(EvaluateDocumentRequestDto request, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(EvaluateDocumentAsync), request.ToJson());
        var response = new EvaluateDocumentResponse(request.CaseId, request.DocumentId, request.VersionId);
        
        var blobSearchResult = await _blobStorageService.FindBlobsByPrefixAsync(request.ProposedBlobName, correlationId);
        var blobInfo = blobSearchResult.FirstOrDefault();

        if (blobInfo == null || request.VersionId != blobInfo.VersionId)
        {
            response.EvaluationResult = DocumentEvaluationResult.AcquireDocument;
        }
        else
        {
            response.EvaluationResult = DocumentEvaluationResult.DocumentUnchanged;
        }
        
        return response;
    }
}
