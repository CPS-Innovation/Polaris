using System.Collections.Generic;

namespace Common.Services.PiiService.Domain
{
    public class PiiEntitiesResultCollection
    {
        public List<PiiEntitiesResult> Items { get; set; }

        public PiiTextDocumentBatchStatistics Statistics { get; set; }

        public string ModelVersion { get; set; }
    }
}