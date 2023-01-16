using System;
using System.IO;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;

namespace Common.Services.DocumentExtractionService.Contracts;

public interface IDdeiDocumentExtractionService
{
    Task<CaseDocument[]> ListDocumentsAsync(string caseUrn, string caseId, string accessToken, string upstreamToken, Guid correlationId);
        
    Task<Stream> GetDocumentAsync(string caseUrn, string caseId, string documentCategory, string documentId, string accessToken, string upstreamToken, Guid correlationId);
}