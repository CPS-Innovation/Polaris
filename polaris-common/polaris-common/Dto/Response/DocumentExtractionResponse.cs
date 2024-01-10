using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace polaris_common.Dto.Response
{
    [ExcludeFromCodeCoverage]
    public class DocumentExtractionResponse
    {
        [JsonProperty("caseId")]
        public string CaseId { get; set; }

        [JsonProperty("documentSasDetails")]
        public DocumentSasDetails DocumentSasDetails { get; set; }
    }
}
