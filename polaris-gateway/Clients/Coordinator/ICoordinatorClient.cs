using Common.Dto.Request;
using Common.ValueObjects;

namespace PolarisGateway.Clients.Coordinator;

public interface ICoordinatorClient
{
    Task<HttpResponseMessage> GetUrnFromCaseIdAsync(int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetCasesAsync(string caseUrn, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId);
    Task<HttpResponseMessage> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId);
    Task<HttpResponseMessage> CheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> CancelCheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> SaveRedactionsAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId);
    Task<HttpResponseMessage> GetCaseSearchIndexCount(string caseUrn, int caseId, Guid correlationId);
    Task<HttpResponseMessage> GetDocumentNotes(string caseUrn, int caseId, string cmsAuthValues, int documentId, Guid correlationId);
    Task<HttpResponseMessage> AddDocumentNote(string caseUrn, int caseId, string cmsAuthValues, int documentId, AddDocumentNoteDto addDocumentNoteRequestDto, Guid correlationId);
    Task<HttpResponseMessage> GetPii(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId);
    Task<HttpResponseMessage> ModifyDocument(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, ModifyDocumentDto modifyDocumentDto, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> RenameDocumentAsync(string caseUrn, int caseId, string cmsAuthValues, int documentId, RenameDocumentRequestDto renameDocumentRequestDto, Guid correlationId);
    Task<HttpResponseMessage> ReclassifyDocument(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, ReclassifyDocumentDto reclassifyDocumentDto, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetCaseExhibitProducers(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetCaseWitnesses(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetMaterialTypeListAsync(string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetWitnessStatementsAsync(string caseUrn, int caseId, int witnessId, string cmsAuthValues, Guid correlationId);
}