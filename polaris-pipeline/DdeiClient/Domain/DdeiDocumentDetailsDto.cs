using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ddei.Domain
{
    public class DdeiDocumentDetailsDto
    {
        public int Id { get; set; }

        public int? VersionId { get; set; }

        public string OriginalFileName { get; set; }

        public string MimeType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DdeiCmsDocCategory CmsDocCategory { get; set; }

        public int TypeId { get; set; }

        public string Type { get; set; }

        public string Date { get; set; }

        [JsonIgnore()]
        public string Path { get; set; }

        public bool Equals([AllowNull] DdeiDocumentDetailsDto other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}