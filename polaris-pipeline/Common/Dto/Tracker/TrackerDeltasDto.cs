using Common.Dto.Case.PreCharge;
using System.Collections.Generic;
using System.Linq;

namespace Common.Dto.Tracker
{
    public class TrackerDeltasDto
    {
        // Cms Document
        public List<TrackerCmsDocumentDto> CreatedDocuments { get; set; }
        public List<TrackerCmsDocumentDto> UpdatedDocuments { get; set; }
        public List<TrackerCmsDocumentDto> DeletedDocuments { get; set; }

        // PCD Requests
        public List<TrackerPcdRequestDto> CreatedPcdRequests { get; set; }
        public List<TrackerPcdRequestDto> UpdatedPcdRequests { get; set; }
        public List<TrackerPcdRequestDto> DeletedPcdRequests { get; set; }

        public bool Any()
        {
            return CreatedDocuments.Any() || UpdatedDocuments.Any() || DeletedDocuments.Any() ||
                   CreatedPcdRequests.Any() || UpdatedPcdRequests.Any() || DeletedPcdRequests.Any();
        }
    }
}
