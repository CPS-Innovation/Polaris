using PolarisGateway.Models;

namespace DdeiClient.Services.DocumentRetrieval;

public interface IDocumentRetrievalService
{
    Task<DocumentRetrievalDto> GetDocumentAsync(string cmsAuthValues, Guid correlationId, string caseUrn, int caseId, string documentId, long versionId);
}