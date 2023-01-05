using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RumpoleGateway.CaseDataImplementations.Tde.Domain
{
    public class DocumentDetails
    {

        public int Id { get; set; }

        public int? VersionId { get; set; }

        public string OriginalFileName { get; set; }

        public string MimeType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CmsDocCategory CmsDocCategory { get; set; }

        public int TypeId { get; set; }

        public string Type { get; set; }

        public string Date { get; set; }

        [JsonIgnore()]
        public string Path { get; set; }

        public bool Equals([AllowNull] DocumentDetails other)
        {
            if (Object.ReferenceEquals(other, null)) return false;
            if (Object.ReferenceEquals(this, other)) return true;

            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}