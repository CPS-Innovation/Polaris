using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using Common.Services.DocumentEvaluation.Contracts;
using Microsoft.Extensions.Logging;

namespace Common.Services.DocumentEvaluation;

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
