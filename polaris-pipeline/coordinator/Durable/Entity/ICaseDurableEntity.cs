using Common.Constants;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using coordinator.Durable.Payloads.Domain;
using System;
using System.Threading.Tasks;

namespace coordinator.Durable.Entity
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    public interface ICaseDurableEntity
    {
        [Obsolete]
        Task<int?> GetVersion();

        [Obsolete]
        void SetVersion(int value);

        [Obsolete]
        void Reset(string TransactionId);

        [Obsolete]
        void SetValue(CaseDurableEntity tracker);
        Task<CaseDeltasEntity> GetCaseDocumentChanges((CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) args);

        [Obsolete]
        void SetDocumentStatus((string DocumentId, DocumentStatus Status, string PdfBlobName) args);

        [Obsolete]
        void SetDocumentConversionStatus((string DocumentId, PdfConversionStatus Status) args);

        void SetCaseStatus((DateTime T, CaseRefreshStatus Status, string Info) args);

        [Obsolete]
        void SetDocumentFlags((string DocumentId, bool IsOcrProcessed, bool IsDispatched) args);

        void SetPiiVersionId(string documentId);

        [Obsolete]
        Task<bool> AllDocumentsFailed();

        [Obsolete]
        Task<string[]> GetDocumentIds();

        [Obsolete]
        Task<DateTime> GetStartTime();

        [Obsolete]
        Task<float> GetDurationToCompleted();

        // vNext stuff
        void SetDocumentPdfConversionSucceeded((string documentId, string pdfBlobName) arg);
        void SetDocumentPdfConversionFailed((string DocumentId, PdfConversionStatus PdfConversionStatus) arg);
        void SetDocumentIndexingSucceeded(string documentId);
        void SetDocumentIndexingFailed(string documentId);
    }
}