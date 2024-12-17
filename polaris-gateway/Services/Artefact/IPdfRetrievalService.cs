using PolarisGateway.Services.Artefact.Domain;

namespace PolarisGateway.Services.Artefact;

public interface IPdfRetrievalService
{
    public Task<DocumentRetrievalResult> GetPdfStreamAsync(string cmsAuthValues, Guid correlationId, string urn, int caseId, string documentId, long versionId);
}