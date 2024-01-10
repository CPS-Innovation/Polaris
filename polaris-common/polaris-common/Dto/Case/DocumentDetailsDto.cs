using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace polaris_common.Dto.Case
{
    public class DocumentDetailsDto
    {
        public int DocumentId { get; set; }

        public int? VersionId { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CmsDocCategory CmsDocCategory { get; set; }

        public DocumentCmsTypeDto CmsDocType { get; set; }

        public string CreatedDate { get; set; }

        [JsonIgnore()]
        public string Path { get; set; }

        public int ParentDocumentId { get; set; }

        public bool Equals([AllowNull] DocumentDetailsDto other)
        {
            if (Object.ReferenceEquals(other, null)) return false;
            if (Object.ReferenceEquals(this, other)) return true;

            return DocumentId.Equals(other.DocumentId);
        }

        public override int GetHashCode()
        {
            return DocumentId.GetHashCode();
        }
    }
}