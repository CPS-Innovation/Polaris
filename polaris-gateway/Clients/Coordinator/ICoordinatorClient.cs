using Common.Dto.Request;

namespace PolarisGateway.Clients.Coordinator;

public interface ICoordinatorClient
{
    Task<HttpResponseMessage> RefreshCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId);
    Task<HttpResponseMessage> DeleteCaseAsync(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> CheckoutDocumentAsync(string caseUrn, int caseId, string documentId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> CancelCheckoutDocumentAsync(string caseUrn, int caseId, string documentId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> SaveRedactionsAsync(string caseUrn, int caseId, string documentId, RedactPdfRequestDto redactPdfRequest, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> SearchCase(string caseUrn, int caseId, string searchTerm, Guid correlationId);
    Task<HttpResponseMessage> GetCaseSearchIndexCount(string caseUrn, int caseId, Guid correlationId);
    Task<HttpResponseMessage> GetDocumentNotes(string caseUrn, int caseId, string cmsAuthValues, string documentId, Guid correlationId);
    Task<HttpResponseMessage> AddDocumentNote(string caseUrn, int caseId, string cmsAuthValues, string documentId, AddDocumentNoteDto addDocumentNoteRequestDto, Guid correlationId);
    Task<HttpResponseMessage> GetPii(string caseUrn, int caseId, string documentId, Guid correlationId);
    Task<HttpResponseMessage> ModifyDocument(string caseUrn, int caseId, string documentId, ModifyDocumentDto modifyDocumentDto, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> RenameDocumentAsync(string caseUrn, int caseId, string cmsAuthValues, string documentId, RenameDocumentRequestDto renameDocumentRequestDto, Guid correlationId);
    Task<HttpResponseMessage> ReclassifyDocument(string caseUrn, int caseId, string documentId, ReclassifyDocumentDto reclassifyDocumentDto, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetCaseExhibitProducers(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetCaseWitnesses(string caseUrn, int caseId, string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetMaterialTypeListAsync(string cmsAuthValues, Guid correlationId);
    Task<HttpResponseMessage> GetWitnessStatementsAsync(string caseUrn, int caseId, int witnessId, string cmsAuthValues, Guid correlationId);
}