using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Common.Dto.Response.Case
{
    public class DefendantsAndChargesListDto : DefendantsAndChargesListCoreDto
    {
        public DefendantsAndChargesListDto()
        { }

        [JsonProperty("defendants")]
        [JsonPropertyName("defendants")]
        public IList<DefendantAndChargesDto> DefendantsAndCharges { get; set; } = new List<DefendantAndChargesDto>();

        public override int DefendantCount => DefendantsAndCharges?.Count ?? 0;
    }
}
