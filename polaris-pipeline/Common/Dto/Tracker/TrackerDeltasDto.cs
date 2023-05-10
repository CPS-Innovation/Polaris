using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Dto.Tracker
{
    public class TrackerDeltasDto
    {
        // Cms Document
        public List<TrackerCmsDocumentDto> CreatedCmsDocuments { get; set; }
        public List<TrackerCmsDocumentDto> UpdatedCmsDocuments { get; set; }
        public List<TrackerCmsDocumentDto> DeletedCmsDocuments { get; set; }

        // PCD Requests
        public List<TrackerPcdRequestDto> CreatedPcdRequests { get; set; }
        public List<TrackerPcdRequestDto> UpdatedPcdRequests { get; set; }
        public List<TrackerPcdRequestDto> DeletedPcdRequests { get; set; }

        // Defendants And Charges
        public TrackerDefendantsAndChargesDto CreatedDefendantsAndCharges { get; set; }
        public TrackerDefendantsAndChargesDto UpdatedDefendantsAndCharges { get; set; }
        public bool IsDeletedDefendantsAndCharges { get; set; }

        public int CreatedCount { get { return CreatedCmsDocuments.Count + CreatedPcdRequests.Count + (CreatedDefendantsAndCharges != null ? 1 : 0); } }
        public int UpdatedCount { get { return UpdatedCmsDocuments.Count + UpdatedPcdRequests.Count + (UpdatedDefendantsAndCharges != null ? 1 : 0); } }
        public int DeletedCount { get { return DeletedCmsDocuments.Count + DeletedPcdRequests.Count + (IsDeletedDefendantsAndCharges ? 1 : 0); } }

        public bool Any()
        {
            return CreatedCmsDocuments.Any() || UpdatedCmsDocuments.Any() || DeletedCmsDocuments.Any() ||
                   CreatedPcdRequests.Any() || UpdatedPcdRequests.Any() || DeletedPcdRequests.Any() ||
                   (CreatedDefendantsAndCharges != null) || (UpdatedDefendantsAndCharges != null) || IsDeletedDefendantsAndCharges;
        }
    }
}
