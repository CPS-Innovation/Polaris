using System.Collections.Generic;
using Common.Dto.FeatureFlags;
using Newtonsoft.Json;

namespace Common.Dto.Case
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
