using coordinator.Durable.Payloads.Domain;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using Common.ValueObjects;
using System;
using System.Threading.Tasks;

namespace coordinator.Durable.Entity
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    public interface ICaseDurableEntity
    {
        Task<int> GetVersion();
        Task<int> IncrementVersion();
        void Reset(string TransactionId);
        Task<CaseDeltasEntity> GetCaseDocumentChanges((CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) args);
        void SetDocumentStatus((PolarisDocumentId PolarisDocumentId, DocumentStatus Status) args);
        void SetDocumentPdfBlobName((PolarisDocumentId PolarisDocumentId, string PdfBlobName) args);
        void SetCaseStatus((DateTime T, CaseRefreshStatus Status, string Info) args);
    }
}