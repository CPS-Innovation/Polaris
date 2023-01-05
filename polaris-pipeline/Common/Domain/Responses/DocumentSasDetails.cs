using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Common.Domain.Responses
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
