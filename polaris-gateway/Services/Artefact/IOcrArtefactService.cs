
using Common.Domain.Ocr;
using PolarisGateway.Services.Artefact.Domain;

namespace PolarisGateway.Services.Artefact;

public interface IOcrArtefactService
{
    public Task<ArtefactResult<AnalyzeResults>> GetOcrAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null);
}