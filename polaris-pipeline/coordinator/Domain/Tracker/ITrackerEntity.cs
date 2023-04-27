using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using coordinator.Functions.DurableEntity.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Domain.Tracker
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    public interface ITrackerEntity
    {
        Task Reset((DateTime t, string transactionId) arg);
        Task SetValue(TrackerEntity tracker);
        Task<TrackerDeltasDto> SynchroniseDocuments((DateTime CurrentUtcDateTime, string CmsCaseUrn, long CmsCaseId, DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantAndChargesDto[] DefendantsAndCharges, Guid CorrelationId) arg);
        Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg);
        Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg);
        Task RegisterUnableToConvertDocumentToPdf((DateTime t, string documentId) arg);
        Task RegisterUnexpectedPdfDocumentFailure((DateTime t, string documentId) arg);
        Task RegisterUnexpectedPdfPcdRequestFailure((DateTime t, int id) arg);
        Task RegisterIndexed((DateTime t, string documentId) arg);
        Task RegisterOcrAndIndexFailure((DateTime t, string documentId) arg);
        Task RegisterCompleted(DateTime t);
        Task RegisterFailed(DateTime t);
        Task RegisterDeleted(DateTime t);
        Task<List<TrackerCmsDocumentDto>> GetDocuments();
        Task ClearDocuments();
        Task<bool> AnyDocumentsFailed();
        Task<bool> AllDocumentsFailed();
    }
}