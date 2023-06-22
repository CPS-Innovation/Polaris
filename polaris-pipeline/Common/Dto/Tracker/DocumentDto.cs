using Newtonsoft.Json;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using System.ComponentModel.DataAnnotations;
using Common.ValueObjects;
using Mapster;
using Newtonsoft.Json.Converters;

namespace Common.Dto.Tracker
{
    public class DocumentDto 
    {
        public DocumentDto()
        { }

        [JsonIgnore]
        public PolarisDocumentId PolarisDocumentId { get; set; }

        [JsonProperty("polarisDocumentId")]
        public string PolarisDocumentIdValue
        {
            get
            {
                return PolarisDocumentId.ToString();
            }
            set
            {
                PolarisDocumentId = new PolarisDocumentId(value);
            }
        }

        [JsonProperty("polarisDocumentVersionId")]
        public int PolarisDocumentVersionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public DocumentStatus Status { get; set; }

        [JsonProperty("cmsDocumentId")]
        [AdaptIgnore]
        public string CmsDocumentId { get; set; }

        [JsonProperty("cmsDocType")]
        public DocumentTypeDto CmsDocType { get; set; }

        [JsonProperty("cmsOriginalFileName")]
        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string CmsOriginalFileName { get; set; }

        [JsonProperty("presentationTitle")]
        public string PresentationTitle { get; set; }

        [JsonProperty("cmsFileCreatedDate")]
        public string CmsFileCreatedDate { get; set; }

        [JsonProperty("isPdfAvailable")]
        public bool IsPdfAvailable { get; set; }

        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }

        [JsonProperty("isOcrProcessed")]
        public bool IsOcrProcessed { get; set; }

        [JsonProperty("categoryListOrder")]
        public int? CategoryListOrder { get; set; }

        [JsonProperty("presentationFlags")]
        public PresentationFlagsDto PresentationFlags { get; set; }
    }
}