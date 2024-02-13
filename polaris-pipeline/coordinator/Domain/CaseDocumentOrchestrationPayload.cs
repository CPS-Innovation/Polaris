using System;
using System.IO;
using System.Text.Json;
using coordinator.Domain.Entity;

namespace coordinator.Domain
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
                string docId;

                if (CmsDocumentTracker != null)
                {
                    docId = CmsDocumentTracker.CmsDocumentId;
                }
                else if (PcdRequestTracker != null)
                {
                    docId = PcdRequestTracker.CmsDocumentId;
                }
                else if (DefendantAndChargesTracker != null)
                {
                    docId = DefendantAndChargesTracker.CmsDocumentId;
                }
                else
                {
                    throw new Exception("No document tracker found");
                }

                return $"{CmsCaseId}/pdfs/CMS-{Path.GetFileNameWithoutExtension(docId)}.pdf";
            }
        }

        public CmsDocumentEntity CmsDocumentTracker { get; set; }
        public PcdRequestEntity PcdRequestTracker { get; set; }
        public DefendantsAndChargesEntity DefendantAndChargesTracker { get; set; }
    }
}
