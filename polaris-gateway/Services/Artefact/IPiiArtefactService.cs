using Common.Domain.Pii;
using PolarisGateway.Services.Artefact.Domain;

namespace PolarisGateway.Services.Artefact;

public interface IPiiArtefactService
{
    public Task<ArtefactResult<IEnumerable<PiiLine>>> GetPiiAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null);
}