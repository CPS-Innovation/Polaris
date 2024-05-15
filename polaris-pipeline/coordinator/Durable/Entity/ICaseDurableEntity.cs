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
        void SetDocumentStatus((string PolarisDocumentId, DocumentStatus Status, string PdfBlobName) args);

        [Obsolete]
        void SetDocumentConversionStatus((string PolarisDocumentId, PdfConversionStatus Status) args);

        void SetCaseStatus((DateTime T, CaseRefreshStatus Status, string Info) args);

        [Obsolete]
        void SetDocumentFlags((string PolarisDocumentId, bool IsOcrProcessed, bool IsDispatched) args);

        [Obsolete]
        Task<bool> AllDocumentsFailed();

        [Obsolete]
        Task<string[]> GetPolarisDocumentIds();

        [Obsolete]
        Task<DateTime> GetStartTime();

        [Obsolete]
        Task<float> GetDurationToCompleted();

        // vNext stuff
        void SetDocumentPdfConversionSucceeded(string polarisDocumentId);
        void SetDocumentPdfConversionFailed((string PolarisDocumentId, PdfConversionStatus PdfConversionStatus) arg);
        void SetDocumentIndexingSucceeded(string polarisDocumentId);
        void SetDocumentIndexingFailed(string polarisDocumentId);
    }
}