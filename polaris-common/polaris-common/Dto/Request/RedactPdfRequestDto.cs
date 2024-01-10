using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using polaris_common.Dto.Request.Redaction;
using polaris_common.ValueObjects;

namespace polaris_common.Dto.Request
{
    public class RedactPdfRequestDto
    {
        public long CaseId { get; set; }

        [JsonIgnore]
        public PolarisDocumentId PolarisDocumentId { get; set; }

        [JsonPropertyName("PolarisDocumentId")]
        public string PolarisDocumentIdValue
        {
            get
            {
                return PolarisDocumentId?.ToString();
            }
            set
            {
                PolarisDocumentId = new PolarisDocumentId(value);
            }
        }

        public long VersionId { get; set; }

        public string FileName { get; set; }

        public List<RedactionDefinitionDto> RedactionDefinitions { get; set; }
    }
}
