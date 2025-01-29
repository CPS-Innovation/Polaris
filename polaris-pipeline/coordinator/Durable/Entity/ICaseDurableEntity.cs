using coordinator.Domain;
using coordinator.Durable.Payloads.Domain;
using System;
using System.Threading.Tasks;

namespace coordinator.Durable.Entity
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    public interface ICaseDurableEntity
    {
        void Reset();

        Task<CaseDeltasEntity> GetCaseDocumentChanges(GetCaseDocumentsResponse getCaseDocumentsResponse);

        void SetCaseStatus(SetCaseStatusPayload payload);

        Task<bool> SetPiiVersionIdAsync(string documentId);

        Task<DateTime> GetStartTime();

        CaseDurableEntityState GetState();

        // vNext stuff
        Task<bool> SetDocumentPdfConversionSucceededAsync(string documentId);

        Task<bool> SetDocumentPdfConversionFailedAsync(SetDocumentPdfConversionFailedPayload payload);

        Task<bool> SetDocumentIndexingSucceededAsync(string documentId);

        Task<bool> SetDocumentIndexingFailedAsync(string documentId);
    }
}