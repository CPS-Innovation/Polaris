using System;
using System.Threading.Tasks;
using Common.Dto.Request;
using Common.Dto.Response;

namespace Common.Services.DocumentEvaluation.Contracts;

public interface IDocumentEvaluationService
{
    Task<EvaluateDocumentResponse> EvaluateDocumentAsync(EvaluateDocumentRequestDto request, Guid correlationId);
}
