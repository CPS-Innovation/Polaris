using PolarisGateway.Services.Domain;

namespace PolarisGateway.Services;

public interface IArtefactService
{
    Task<PdfResult> GetPdf(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed);
    Task<JsonArtefactResult> GetOcr(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null);
    Task<JsonArtefactResult> GetPII(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed, Guid? operationId = null);
}