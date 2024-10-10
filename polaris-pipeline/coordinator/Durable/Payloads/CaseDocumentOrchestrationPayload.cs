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
                string cmsCaseUrn,
                int cmsCaseId,
                string serializedTrackerCmsDocumentDto,
                string serializedTrackerPcdRequestDto,
                string serializedTrackerDefendantAndChargesDto,
                DocumentDeltaType documentDeltaType
            )
            : base(cmsCaseUrn, cmsCaseId, correlationId)
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

        public string CmsDocumentId
        {
            get
            {
                if (CmsDocumentTracker != null)
                    return CmsDocumentTracker.CmsDocumentId;
                if (PcdRequestTracker != null)
                    return PcdRequestTracker.CmsDocumentId;
                else
                    return DefendantAndChargesTracker.CmsDocumentId ?? string.Empty;
            }
        }

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

        public string DocumentTypeId
        {
            get
            {
                if (CmsDocumentTracker != null)
                    return CmsDocumentTracker.CmsDocType.DocumentTypeId;
                else
                    return string.Empty;
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
                return !string.IsNullOrEmpty(CmsDocumentId)
                    ? BlobNameHelper.GetBlobName(CmsCaseId, CmsDocumentId, BlobNameHelper.BlobType.Pdf)
                    : throw new Exception("No document tracker found");
            }
        }

        public string OcrBlobName
        {
            get
            {
                return !string.IsNullOrEmpty(CmsDocumentId)
                    ? BlobNameHelper.GetBlobName(CmsCaseId, CmsDocumentId, BlobNameHelper.BlobType.Ocr)
                    : throw new Exception("No document tracker found");
            }
        }

        public CmsDocumentEntity CmsDocumentTracker { get; set; }
        public PcdRequestEntity PcdRequestTracker { get; set; }
        public DefendantsAndChargesEntity DefendantAndChargesTracker { get; set; }
    }
}
