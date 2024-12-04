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

        void SetPiiVersionId(string documentId);

        Task<DateTime> GetStartTime();

        // vNext stuff
        void SetDocumentPdfConversionSucceeded(string documentId);

        void SetDocumentPdfConversionFailed(SetDocumentPdfConversionFailedPayload payload);

        void SetDocumentIndexingSucceeded(string documentId);

        void SetDocumentIndexingFailed(string documentId);
    }
}