using Newtonsoft.Json;
using polaris_common.Dto.FeatureFlags;

namespace polaris_common.Dto.Case
{
    public class DefendantsAndChargesListDto
    {
        [JsonProperty("caseId")]
        public int CaseId { get; set; }

        [JsonProperty("defendants")]
        public IEnumerable<DefendantAndChargesDto> DefendantsAndCharges { get; set; }

        public PresentationFlagsDto PresentationFlags { get; set; }
    }
}
