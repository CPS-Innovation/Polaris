using System;
using System.Text.Json;
using coordinator.Helpers;
using coordinator.Durable.Payloads.Domain;

namespace coordinator.Durable.Payloads
{
    public class CaseDocumentOrchestrationPayload : BasePipelinePayload
    {
        public CaseDocumentOrchestrationPayload
            (
                string cmsAuthValues,
                Guid correlationId,
                Guid subCorrelationId,
                string urn,
                int caseId,
                string serializedTrackerCmsDocumentDto,
                string serializedTrackerPcdRequestDto,
                string serializedTrackerDefendantAndChargesDto,
                DocumentDeltaType documentDeltaType
            )
            : base(urn, caseId, correlationId)
        {
            if (serializedTrackerCmsDocumentDto != null)
            {
                CmsDocumentTracker = JsonSerializer.Deserialize<CmsDocumentEntity>(serializedTrackerCmsDocumentDto);
                DocumentId = CmsDocumentTracker.DocumentId;
            }
            else if (serializedTrackerPcdRequestDto != null)
            {
                PcdRequestTracker = JsonSerializer.Deserialize<PcdRequestEntity>(serializedTrackerPcdRequestDto);
                DocumentId = PcdRequestTracker.DocumentId;
            }
            else if (serializedTrackerDefendantAndChargesDto != null)
            {
                DefendantAndChargesTracker = JsonSerializer.Deserialize<DefendantsAndChargesEntity>(serializedTrackerDefendantAndChargesDto);
                DocumentId = DefendantAndChargesTracker.DocumentId;
            }
            SubCorrelationId = subCorrelationId;
            CmsAuthValues = cmsAuthValues;
            DocumentDeltaType = documentDeltaType;
        }

        public Guid? SubCorrelationId { get; set; }

        public string CmsAuthValues { get; set; }

        public DocumentDeltaType DocumentDeltaType { get; set; }

        public long VersionId
        {
            get
            {
                if (CmsDocumentTracker != null)
                    return CmsDocumentTracker.VersionId;
                if (PcdRequestTracker != null)
                    return PcdRequestTracker.VersionId;
                else
                    return DefendantAndChargesTracker.VersionId;
            }
        }

        public int? DocumentTypeId
        {
            get
            {
                return CmsDocumentTracker?.CmsDocType?.DocumentTypeId;
            }
        }

        public string DocumentType
        {
            get
            {
                if (CmsDocumentTracker != null)
                    return CmsDocumentTracker.CmsDocType.DocumentType;
                else
                    return string.Empty;
            }
        }

        public string DocumentCategory
        {
            get
            {
                if (CmsDocumentTracker != null)
                    return CmsDocumentTracker.CmsDocType.DocumentCategory;
                else
                    return string.Empty;
            }
        }

        public string BlobName
        {
            get
            {
                return !string.IsNullOrEmpty(DocumentId)
                    ? BlobNameHelper.GetBlobName(CaseId, DocumentId, BlobNameHelper.BlobType.Pdf)
                    : throw new Exception("No document tracker found");
            }
        }

        public string OcrBlobName
        {
            get
            {
                return !string.IsNullOrEmpty(DocumentId)
                    ? BlobNameHelper.GetBlobName(CaseId, DocumentId, BlobNameHelper.BlobType.Ocr)
                    : throw new Exception("No document tracker found");
            }
        }

        public CmsDocumentEntity CmsDocumentTracker { get; set; }
        public PcdRequestEntity PcdRequestTracker { get; set; }
        public DefendantsAndChargesEntity DefendantAndChargesTracker { get; set; }
    }
}
