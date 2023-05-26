using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using System;
using System.Threading.Tasks;

namespace coordinator.Functions.DurableEntity.Entity.Contract
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    public interface ICaseEntity
    {
        Task<int> GetVersion();
        void SetVersion(int value);

        void Reset(string TransactionId);
        void SetValue(CaseEntity tracker);
        Task<TrackerDeltasDto> GetCaseDocumentChanges((DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) args);
        void RegisterDocumentStatus((string PolarisDocumentId, TrackerDocumentStatus Status, string PdfBlobName) arg);
        void RegisterCompleted((DateTime T, bool Success) arg);
        Task<bool> AllDocumentsFailed();
    }
}