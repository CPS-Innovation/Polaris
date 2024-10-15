namespace PolarisGateway.Services;

public interface IArtefactService
{
    Task<Stream> GetPdf(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId, bool isOcrProcessed);
    Task<Stream> InitiateOcr(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId);
}