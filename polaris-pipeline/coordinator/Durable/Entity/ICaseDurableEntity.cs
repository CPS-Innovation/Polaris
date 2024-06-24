using Common.Constants;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using coordinator.Durable.Payloads.Domain;
using System;
using System.Threading.Tasks;

namespace coordinator.Durable.Entity
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    public interface ICaseDurableEntity
    {
        Task<CaseDeltasEntity> MutateAndReturnDeltas((CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) args);
        Task<int> InitialiseRefresh();
        void SetCaseDocumentsRetrieved(DateTime args);
        void SetCaseCompleted(DateTime args);
        void SetCaseFailed(DateTime args);
        void SetDocumentPdfConversionSucceeded((string polarisDocumentId, string pdfBlobName) arg);
        void SetDocumentPdfConversionFailed((string PolarisDocumentId, PdfConversionStatus PdfConversionStatus) arg);
        void SetDocumentIndexingSucceeded(string polarisDocumentId);
        void SetDocumentIndexingFailed(string polarisDocumentId);
        void SetPiiCmsVersionId(string polarisDocumentId);
    }
}