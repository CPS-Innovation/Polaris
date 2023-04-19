using Common.Dto.Case.PreCharge;
using System.Collections.Generic;
using System.Linq;

namespace Common.Dto.Tracker
{
    public class TrackerDeltasDto
    {
        public List<TrackerDocumentDto> CreatedDocuments { get; set; }
        public List<TrackerDocumentDto> UpdatedDocuments { get; set; }
        public List<TrackerDocumentDto> DeletedDocuments { get; set; }

        // Read only, so Created and Updated only options
        public List<TrackerPcdRequestDto> CreatedPcdRequests { get; set; }
        public List<TrackerPcdRequestDto> DeletedPcdRequests { get; set; }

        public bool Any()
        {
            return CreatedDocuments.Any() || UpdatedDocuments.Any() || DeletedDocuments.Any() ||
                   CreatedPcdRequests.Any() || DeletedPcdRequests.Any();
        }
    }
}
