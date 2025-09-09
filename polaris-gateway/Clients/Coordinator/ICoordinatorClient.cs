using Common.Dto.Request;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace PolarisGateway.Clients.Coordinator;

public interface ICoordinatorClient
{
    Task<HttpResponseMessage> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);

    Task<HttpResponseMessage> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId);

    Task<HttpResponseMessage> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);

    Task<HttpResponseMessage> SaveRedactionsAsync(string caseUrn, int caseId, string documentId, long versionId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId);

    Task<HttpResponseMessage> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId);

    Task<HttpResponseMessage> GetCaseSearchIndexCount(string caseUrn, int caseId, Guid correlationId);

    Task<HttpResponseMessage> ModifyDocument(string caseUrn, int caseId, string documentId, long versionId, ModifyDocumentDto modifyDocumentDto, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> BulkRedactionSearchAsync(string caseUrn, int caseId, string documentId, long versionId, string searchText, Guid correlationId, string cmsAuthValues, CancellationToken cancellationToken = default);
}