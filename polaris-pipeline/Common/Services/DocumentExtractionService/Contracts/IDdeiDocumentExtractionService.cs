using System;
using System.IO;
using System.Threading.Tasks;
using Common.Dto.Document;

namespace Common.Services.DocumentExtractionService.Contracts;

public interface IDdeiDocumentExtractionService
{
    Task<DocumentDto[]> ListDocumentsAsync(string caseUrn, string caseId, string cmsAuthValues, Guid correlationId);

    Task<Stream> GetDocumentAsync(string caseUrn, string caseId, string documentCategory, string documentId, string cmsAuthValues, Guid correlationId);
}