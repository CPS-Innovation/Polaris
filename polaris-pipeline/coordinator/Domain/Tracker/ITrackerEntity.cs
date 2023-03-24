using Common.Domain.Case;
using coordinator.Functions.DurableEntity.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Domain.Tracker
{
    public interface ITrackerEntity
    {
        Task Reset(string transactionId);
        Task SetValue(TrackerEntity tracker);
        Task<TrackerDocumentListDeltas> SynchroniseDocuments(SynchroniseDocumentsArg arg);
        Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg);
        Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg);
        Task RegisterUnableToConvertDocumentToPdf(string documentId);
        Task RegisterUnexpectedPdfDocumentFailure(string documentId);
        Task RegisterIndexed(string documentId);
        Task RegisterOcrAndIndexFailure(string documentId);
        Task RegisterCompleted(bool changed);
        Task RegisterFailed();
        Task RegisterDeleted();
        Task<List<TrackerDocument>> GetDocuments();
        Task ClearDocuments();
        Task<bool> AllDocumentsFailed();
    }
}