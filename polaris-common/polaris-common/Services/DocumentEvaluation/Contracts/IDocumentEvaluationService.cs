using polaris_common.Dto.Request;
using polaris_common.Dto.Response;

namespace polaris_common.Services.DocumentEvaluation.Contracts;

public interface IDocumentEvaluationService
{
    Task<EvaluateDocumentResponse> EvaluateDocumentAsync(EvaluateDocumentRequestDto request, Guid correlationId);
}
