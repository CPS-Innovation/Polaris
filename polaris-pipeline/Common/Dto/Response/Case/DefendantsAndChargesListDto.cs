using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Dto.Response.Document.FeatureFlags;

namespace Common.Dto.Response.Case
{
    public class DefendantsAndChargesListDto
    {
        public DefendantsAndChargesListDto()
        {
        }

        [JsonPropertyName("caseId")]
        public int CaseId { get; set; }

        [JsonPropertyName("versionId")]
        public long VersionId { get; set; }

        [JsonPropertyName("defendants")]
        public IEnumerable<DefendantAndChargesDto> DefendantsAndCharges { get; set; } = [];

        public PresentationFlagsDto PresentationFlags { get; set; } = new PresentationFlagsDto();
    }
}
