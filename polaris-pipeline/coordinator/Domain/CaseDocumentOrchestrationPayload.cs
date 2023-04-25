using Common.Dto.Tracker;
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
                string serializedTrackerPcdRequestDto
            )
            : base(cmsCaseUrn, cmsCaseId, correlationId)
        {
            if(serializedTrackerCmsDocumentDto != null)
            {
                CmsDocumentTracker = JsonSerializer.Deserialize<TrackerCmsDocumentDto>(serializedTrackerCmsDocumentDto);
                base.PolarisDocumentId = CmsDocumentTracker.PolarisDocumentId;
            }
            else if(serializedTrackerPcdRequestDto != null)
            {
                PcdRequestTracker = JsonSerializer.Deserialize<TrackerPcdRequestDto>(serializedTrackerPcdRequestDto);
                base.PolarisDocumentId = PcdRequestTracker.PolarisDocumentId;
            }
            CmsAuthValues = cmsAuthValues;
        }
        public string CmsAuthValues { get; set; }

        public string CmsDocumentId 
        { 
            get
            {
                return CmsDocumentTracker != null ? CmsDocumentTracker.CmsDocumentId : PcdRequestTracker.CmsDocumentId;
            }
        }

        public long CmsVersionId
        {
            get
            {
                return CmsDocumentTracker != null ? CmsDocumentTracker.CmsVersionId : PcdRequestTracker.CmsVersionId;
            }
        }

        public TrackerCmsDocumentDto CmsDocumentTracker { get; set; }

        public TrackerPcdRequestDto PcdRequestTracker { get; set; } 
    }
}
