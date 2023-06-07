using Newtonsoft.Json;

namespace Common.Dto.Tracker
{
    public class TrackerDocumentLogDto
    {
        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("updated")]
        public string Updated { get; set; }

        [JsonProperty("deleted")]
        public string Deleted { get; set; }

        [JsonProperty("pdfGenerated")]
        public float? PdfGenerated { get; set; }

        [JsonProperty("pdfAlreadyGenerated")]
        public float? PdfAlreadyGenerated { get; set; }

        [JsonProperty("indexed")]
        public float? Indexed { get; set; }
    }
}