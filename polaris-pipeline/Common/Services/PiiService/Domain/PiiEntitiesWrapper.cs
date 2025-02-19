using System.Collections.Generic;

namespace Common.Services.PiiService.Domain
{
    public class PiiEntitiesWrapper
    {
        public PiiEntitiesWrapper()
        {
            PiiResultCollection = [];
        }

        public IEnumerable<PiiEntitiesResultCollection> PiiResultCollection { get; set; }
    }
}