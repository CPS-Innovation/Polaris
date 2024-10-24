using Common.Dto.Request;

namespace PolarisGateway.Clients.Coordinator;

public interface ICoordinatorClient
{
    Task<HttpResponseMessage> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId);
    Task<HttpResponseMessage> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> SaveRedactionsAsync(string caseUrn, int caseId, string documentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId);
    Task<HttpResponseMessage> GetCaseSearchIndexCount(string caseUrn, int caseId, Guid correlationId);
    Task<HttpResponseMessage> ModifyDocument(string caseUrn, int caseId, string documentId, ModifyDocumentDto modifyDocumentDto, string cmsAuthValues, Guid correlationId);
}