using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Dto.Response.Document.FeatureFlags;
using Newtonsoft.Json;

namespace Common.Dto.Response.Case
{
    public class DefendantsAndChargesListDto
    {
        public DefendantsAndChargesListDto()
        {

        }

        [JsonProperty("caseId")]
        [JsonPropertyName("caseId")]
        public int CaseId { get; set; }

        [JsonProperty("versionId")]
        [JsonPropertyName("versionId")]
        public long VersionId { get; set; }

        [JsonProperty("defendants")]
        [JsonPropertyName("defendants")]
        public IEnumerable<DefendantAndChargesDto> DefendantsAndCharges { get; set; } = new List<DefendantAndChargesDto>();

        public PresentationFlagsDto PresentationFlags { get; set; } = new PresentationFlagsDto();
    }
}
