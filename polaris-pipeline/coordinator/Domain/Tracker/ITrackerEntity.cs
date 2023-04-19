using Common.Dto.Tracker;
using coordinator.Functions.DurableEntity.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Domain.Tracker
{
    public interface ITrackerEntity
    {
        Task Reset(string transactionId);
        Task SetValue(TrackerEntity tracker);
        Task<TrackerDeltasDto> SynchroniseDocuments(SynchroniseDocumentsArg arg);
        Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg);
        Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg);
        Task RegisterUnableToConvertDocumentToPdf(string documentId);
        Task RegisterUnexpectedPdfDocumentFailure(string documentId);
        Task RegisterUnexpectedPdfPcdRequestFailure(int id);
        Task RegisterIndexed(string documentId);
        Task RegisterOcrAndIndexFailure(string documentId);
        Task RegisterCompleted();
        Task RegisterFailed();
        Task RegisterDeleted();
        Task<List<TrackerDocumentDto>> GetDocuments();
        Task ClearDocuments();
        Task<bool> AllDocumentsFailed();
    }
}