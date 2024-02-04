using coordinator.Durable.Payloads.Domain;
using coordinator.Domain;
using System;
using System.Text.Json;

namespace coordinator.Durable.Payloads
{
    public class CaseDocumentOrchestrationPayload : BasePipelinePayload
    {
        public CaseDocumentOrchestrationPayload
            (
                string cmsAuthValues,
                Guid correlationId,
                string cmsCaseUrn,
                long cmsCaseId,
                string serializedTrackerCmsDocumentDto,
                string serializedTrackerPcdRequestDto,
                string serializedTrackerDefendantAndChargesDto
            )
            : base(cmsCaseUrn, cmsCaseId, correlationId)
        {
            if (serializedTrackerCmsDocumentDto != null)
            {
                CmsDocumentTracker = JsonSerializer.Deserialize<CmsDocumentEntity>(serializedTrackerCmsDocumentDto);
                PolarisDocumentId = CmsDocumentTracker.PolarisDocumentId;
            }
            else if (serializedTrackerPcdRequestDto != null)
            {
                PcdRequestTracker = JsonSerializer.Deserialize<PcdRequestEntity>(serializedTrackerPcdRequestDto);
                PolarisDocumentId = PcdRequestTracker.PolarisDocumentId;
            }
            else if (serializedTrackerDefendantAndChargesDto != null)
            {
                DefendantAndChargesTracker = JsonSerializer.Deserialize<DefendantsAndChargesEntity>(serializedTrackerDefendantAndChargesDto);
                PolarisDocumentId = DefendantAndChargesTracker.PolarisDocumentId;
            }
            CmsAuthValues = cmsAuthValues;
        }
        public string CmsAuthValues { get; set; }

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

        public long CmsVersionId
        {
            get
            {
                if (CmsDocumentTracker != null)
                    return CmsDocumentTracker.CmsVersionId;
                if (PcdRequestTracker != null)
                    return PcdRequestTracker.CmsVersionId;
                else
                    return DefendantAndChargesTracker.CmsVersionId;
            }
        }

        public string BlobName
        {
            get
            {
                var cmsDocId = CmsDocumentId;
                if (string.IsNullOrEmpty(cmsDocId))
                {
                    throw new Exception("No document tracker found");
                }

                return PdfBlobNameHelper.GetPdfBlobName(CmsCaseId, cmsDocId);
            }
        }

        public CmsDocumentEntity CmsDocumentTracker { get; set; }
        public PcdRequestEntity PcdRequestTracker { get; set; }
        public DefendantsAndChargesEntity DefendantAndChargesTracker { get; set; }
    }
}
