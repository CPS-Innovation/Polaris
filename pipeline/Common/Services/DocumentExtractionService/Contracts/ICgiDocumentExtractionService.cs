using System;
using System.IO;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;

namespace Common.Services.DocumentExtractionService.Contracts;

public interface ICgiDocumentExtractionService
{
    Task<CaseDocument[]> ListDocumentsAsync(string caseId, string accessToken, Guid correlationId);
        
    Task<Stream> GetDocumentAsync(string documentId, string fileName, string accessToken, Guid correlationId);
}
