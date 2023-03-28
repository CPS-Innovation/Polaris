using System.Collections.Generic;
using System.Linq;
using Common.Dto.Document;

namespace Common.Dto.Tracker
{
    public class TrackerDocumentListDeltasDto
    {
        public List<TrackerDocumentDto> CreatedOrUpdated { get; set; }
        public List<DocumentVersionDto> Deleted { get; set; }

        public bool Any()
        {
            return CreatedOrUpdated.Any() || Deleted.Any();
        }
    }
}
