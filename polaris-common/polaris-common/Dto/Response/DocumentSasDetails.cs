using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace polaris_common.Dto.Response
{
    [ExcludeFromCodeCoverage]
    public class DocumentSasDetails
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("documentSasUrl")]
        public string DocumentSasUrl { get; set; }
    }
}
