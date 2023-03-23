using Common.Domain.Case;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Domain.Tracker
{
    public interface ITracker
    {
        Task Reset(string transactionId);
        Task SetValue(Functions.DurableEntityFunctions.Tracker tracker);
        Task SynchroniseDocuments(SynchroniseDocumentsArg arg);
        Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg);
        Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg);
        Task RegisterUnableToConvertDocumentToPdf(string documentId);
        Task RegisterUnexpectedPdfDocumentFailure(string documentId);
        Task RegisterIndexed(string documentId);
        Task RegisterOcrAndIndexFailure(string documentId);
        Task RegisterCompleted();
        Task RegisterFailed();
        Task RegisterDeleted();
        Task<List<TrackerDocument>> GetDocuments();
        Task ClearDocuments();
        Task<bool> AllDocumentsFailed();
    }
}