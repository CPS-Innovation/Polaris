using Common.Domain.Ocr;
using PolarisGateway.Services.Artefact.Domain;
using System;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact;

public interface IOcrArtefactService
{
    public Task<ArtefactResult<AnalyzeResults>> GetOcrAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string materialId, long documentId, bool isOcrProcessed, Guid? operationId = null, bool forceRefresh = false);
}