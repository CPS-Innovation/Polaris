using Common.Domain.Ocr;
using Common.Domain.Pii;
using PolarisGateway.Services.Artefact.Domain;

namespace PolarisGateway.Services.Artefact;

public interface IArtefactService
{
    Task<ArtefactResult<Stream>> GetPdf(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed);
    Task<ArtefactResult<(Guid?, AnalyzeResults)>> GetOcr(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null);
    Task<ArtefactResult<(Guid?, IEnumerable<PiiLine>)>> GetPii(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null);
}