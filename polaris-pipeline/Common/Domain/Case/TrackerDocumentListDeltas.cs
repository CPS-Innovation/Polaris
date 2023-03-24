using System.Collections.Generic;
using System.Linq;

namespace Common.Domain.Case
{
    public class TrackerDocumentListDeltas
    {
        public List<TrackerDocument> CreatedOrUpdated { get; set; }
        public List<DocumentVersion> Deleted { get; set; }

        public bool Any()
        {
            return CreatedOrUpdated.Any() || Deleted.Any();
        }
    }
}
