using System.Collections.Generic;
using System.Linq;

namespace Common.Dto.Tracker
{
    public class TrackerDocumentListDeltasDto
    {
        public List<TrackerDocumentDto> Created { get; set; }
        public List<TrackerDocumentDto> Updated { get; set; }
        public List<TrackerDocumentDto> Deleted { get; set; }

        public bool Any()
        {
            return Created.Any() || Updated.Any() || Deleted.Any();
        }
    }
}
