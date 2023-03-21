using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Domain.Tracker
{
    public interface ITracker
    {
        Task Initialise(string transactionId);
        Task RegisterDocumentIds(RegisterDocumentIdsArg arg);
        Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg);
        Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg);
        Task RegisterDocumentNotFoundInDDEI(string documentId);
        Task RegisterUnableToConvertDocumentToPdf(string documentId);
        Task RegisterUnexpectedPdfDocumentFailure(string documentId);
        Task RegisterNoDocumentsFoundInDDEI();
        Task RegisterIndexed(string documentId);
        Task RegisterOcrAndIndexFailure(string documentId);
        Task RegisterCompleted();
        Task RegisterFailed();
        Task RegisterDeleted();
        Task<List<TrackerDocument>> GetDocuments();
        Task<bool> AllDocumentsFailed();
        Task<bool> IsAlreadyProcessed();
        Task<bool> IsStale(bool forceRefresh);
    }
}