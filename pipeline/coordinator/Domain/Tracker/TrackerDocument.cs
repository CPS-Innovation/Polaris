using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace coordinator.Domain.Tracker
{
    public class TrackerDocument
    {
        public TrackerDocument(string documentId, long versionId, string originalFileName)
        {
            DocumentId = documentId;
            VersionId = versionId;
            OriginalFileName = originalFileName;
            Status = DocumentStatus.None;
        }
        
        [JsonProperty("documentId")]
        public string DocumentId { get; set; }
        
        [JsonProperty("versionId")]
        public long VersionId { get; set; }
        
        [JsonProperty("originalFileName")] 
        public string OriginalFileName { get; set; }

        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public DocumentStatus Status { get; set; }
    }
}