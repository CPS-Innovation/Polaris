using Common.Domain.Ocr;
using Common.Dto.Request.Redaction;
using PolarisGateway.Services.Artefact.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact;

public interface IOcrArtefactService
{
    public Task<ArtefactResult<AnalyzeResults>> GetOcrAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null);

    Task<IEnumerable<RedactionDefinitionDto>> GetOcrSearchRedactionsAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, string searchTerm, CancellationToken cancellationToken = default);
}