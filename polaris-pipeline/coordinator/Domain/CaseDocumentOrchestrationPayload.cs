using Common.Domain.Entity;
using System;
using System.Text.Json;

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
            if(serializedTrackerCmsDocumentDto != null)
            {
                CmsDocumentTracker = JsonSerializer.Deserialize<CmsDocumentEntity>(serializedTrackerCmsDocumentDto);
                base.PolarisDocumentId = CmsDocumentTracker.PolarisDocumentId;
            }
            else if(serializedTrackerPcdRequestDto != null)
            {
                PcdRequestTracker = JsonSerializer.Deserialize<PcdRequestEntity>(serializedTrackerPcdRequestDto);
                base.PolarisDocumentId = PcdRequestTracker.PolarisDocumentId;
            }
            else if (serializedTrackerDefendantAndChargesDto != null)
            {
                DefendantAndChargesTracker = JsonSerializer.Deserialize<DefendantsAndChargesEntity>(serializedTrackerDefendantAndChargesDto);
                base.PolarisDocumentId = DefendantAndChargesTracker.PolarisDocumentId;
            }
            CmsAuthValues = cmsAuthValues;
        }
        public string CmsAuthValues { get; set; }

        public string CmsDocumentId 
        { 
            get
            {
                if(CmsDocumentTracker != null)
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

        public CmsDocumentEntity CmsDocumentTracker { get; set; }
        public PcdRequestEntity PcdRequestTracker { get; set; }
        public DefendantsAndChargesEntity DefendantAndChargesTracker { get; set; }
    }
}
