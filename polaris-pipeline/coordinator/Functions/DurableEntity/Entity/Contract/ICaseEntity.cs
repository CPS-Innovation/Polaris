using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using coordinator.Domain.Tracker;
using System;
using System.Threading.Tasks;

namespace coordinator.Functions.DurableEntity.Entity.Contract
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    public interface ICaseEntity
    {
        Task<int> GetVersion();
        Task SetVersion(int value);

        Task Reset((DateTime t, string transactionId) arg);
        Task ClearDocuments();
        Task SetValue(CaseEntity tracker);
        Task<TrackerDeltasDto> GetCaseDocumentChanges((DateTime CurrentUtcDateTime, string CmsCaseUrn, long CmsCaseId, DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges, Guid CorrelationId) arg);
        Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg);
        Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg);
        Task RegisterStatus((string polarisDocumentId, TrackerDocumentStatus status, TrackerLogType logType) arg);
        void RegisterCompleted((DateTime t, bool success) arg);
        Task<bool> AllDocumentsFailed();
    }
}