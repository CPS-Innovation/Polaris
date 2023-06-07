using Common.Dto.Tracker;
using Newtonsoft.Json;

namespace Common.Domain.Entity
{
    public class DocumentLogEntity
    {
        [JsonProperty("created")]
        public string Created { get; set; } = null;

        [JsonProperty("updated")]
        public string Updated { get; set; } = null;

        [JsonProperty("deleted")]
        public string Deleted { get; set; } = null;

        [JsonProperty("pdfGenerated")]
        public float? PdfGenerated { get; set; } = null;

        [JsonProperty("pdfAlreadyGenerated")]
        public float? PdfAlreadyGenerated { get; set; } = null;

        [JsonProperty("indexed")]
        public float? Indexed { get; set; } = null;

        public string Timestamp
        {
            get
            {
                return Created ?? Updated ?? Deleted;
            }
        }

        public float? GetStatusTime(DocumentLogType status)
        {
            switch (status)
            {
                case DocumentLogType.PdfGenerated:
                    return PdfGenerated;

                case DocumentLogType.PdfAlreadyGenerated:
                    return PdfAlreadyGenerated;

                case DocumentLogType.Indexed:
                    return Indexed;

                default:
                    return null;
            }
        }
    }
}