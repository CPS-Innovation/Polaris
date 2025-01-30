//using System.Collections.Generic;
using System.Text.Json.Serialization;
//using Common.Dto.Response.Document.FeatureFlags;
using Newtonsoft.Json;

namespace Common.Dto.Response.Case
{
    public class DefendantsAndChargesListCoreDto
    {
        public DefendantsAndChargesListCoreDto()
        {

        }

        [JsonProperty("caseId")]
        [JsonPropertyName("caseId")]
        public int CaseId { get; set; }

        [JsonProperty("versionId")]
        [JsonPropertyName("versionId")]
        public long VersionId { get; set; }

        public virtual int DefendantCount
        {
            get;
        }
    }
}
