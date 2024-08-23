using Common.Dto.Request;
using Common.ValueObjects;

namespace PolarisGateway.Clients.Coordinator;

public interface ICoordinatorClient
{
    Task<ContentResult> GetCasesAsync(string caseUrn, string cmsAuthValues, Guid correlationId);
    Task<ContentResult> GetCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<ContentResult> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<IActionResult> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId);
    Task<ContentResult> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<ContentResult> GetDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId);
    Task<ContentResult> CheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId);
    Task<ContentResult> CancelCheckoutDocumentAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, string cmsAuthValues, Guid correlationId);
    Task<ContentResult> SaveRedactionsAsync(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId);
    Task<ContentResult> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId);
    Task<ContentResult> GetCaseSearchIndexCount(string caseUrn, int caseId, Guid correlationId);
    Task<ContentResult> GetDocumentNotes(string caseUrn, int caseId, string cmsAuthValues, int documentId, Guid correlationId);
    Task<ContentResult> AddDocumentNote(string caseUrn, int caseId, string cmsAuthValues, int documentId, AddDocumentNoteDto addDocumentNoteRequestDto, Guid correlationId);
    Task<ContentResult> GetPii(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, Guid correlationId);
    Task<ContentResult> ModifyDocument(string caseUrn, int caseId, PolarisDocumentId polarisDocumentId, ModifyDocumentDto modifyDocumentDto, string cmsAuthValues, Guid correlationId);
    Task<ContentResult> RenameDocumentAsync(string caseUrn, int caseId, string cmsAuthValues, int documentId, RenameDocumentRequestDto renameDocumentRequestDto, Guid correlationId);
    Task<HttpResponseMessage> GetCaseExhibitProducers(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetCaseWitnesses(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<ContentResult> GetMaterialTypeListAsync(string cmsAuthValues, Guid correlationId);
}