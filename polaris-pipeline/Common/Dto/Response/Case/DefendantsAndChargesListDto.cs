using System.Collections.Generic;
using Common.Dto.Response.Document.FeatureFlags;
using Newtonsoft.Json;

namespace Common.Dto.Response.Case
{
    public class DefendantsAndChargesListDto
    {
        [JsonProperty("caseId")]
        public int CaseId { get; set; }

        [JsonProperty("versionId")]
        public long VersionId { get; set; }

        [JsonProperty("defendants")]
        public IEnumerable<DefendantAndChargesDto> DefendantsAndCharges { get; set; } = new List<DefendantAndChargesDto>();

        public PresentationFlagsDto PresentationFlags { get; set; } = new PresentationFlagsDto();
    }
}
