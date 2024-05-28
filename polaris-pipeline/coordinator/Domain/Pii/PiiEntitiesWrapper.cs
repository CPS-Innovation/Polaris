using System.Collections.Generic;

namespace coordinator.Domain
{
    public class PiiEntitiesWrapper
    {
        public PiiEntitiesWrapper()
        {
            PiiResultCollection = new List<PiiEntitiesResultCollection>();
        }

        public IEnumerable<PiiEntitiesResultCollection> PiiResultCollection { get; set; }
    }
}