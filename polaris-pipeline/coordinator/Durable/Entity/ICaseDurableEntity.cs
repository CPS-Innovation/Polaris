using Common.Constants;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Documents;
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
        Task<CaseDeltasEntity> GetCaseDocumentChanges((CmsDocumentCoreDto[] CmsDocuments, PcdRequestCoreDto[] PcdRequests, DefendantsAndChargesListCoreDto DefendantsAndCharges) args);
        void SetCaseStatus((DateTime T, CaseRefreshStatus Status, string Info) args);
        Task<DateTime> GetStartTime();

        // vNext stuff
        void SetDocumentPdfConversionFailed((string DocumentId, PdfConversionStatus PdfConversionStatus) arg);
        void SetDocumentIndexingSucceeded(string documentId);
        void SetDocumentIndexingFailed(string documentId);
    }
}