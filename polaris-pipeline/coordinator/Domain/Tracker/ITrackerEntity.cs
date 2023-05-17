using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using coordinator.Functions.DurableEntity.Entity;
using System;
using System.Threading.Tasks;

namespace coordinator.Domain.Tracker
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    public interface ITrackerEntity
    {
        Task Reset((DateTime t, string transactionId) arg);
        Task ClearDocuments();
        Task SetValue(TrackerEntity tracker);
        Task<TrackerDeltasDto> GetCaseDocumentChanges((DateTime CurrentUtcDateTime, string CmsCaseUrn, long CmsCaseId, DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges, Guid CorrelationId) arg);
        Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg);
        Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg);
        Task RegisterStatus((DateTime t, string documentId, TrackerDocumentStatus status, TrackerLogType logType) arg);
        Task RegisterCompleted((DateTime t, bool success) arg);
        Task<bool> AllDocumentsFailed();
    }
}