using Common.Domain.Document;
using Newtonsoft.Json;

namespace coordinator.Durable.Payloads.Domain
{
    public class CmsDocumentEntity : BaseDocumentEntity
    {
        public CmsDocumentEntity()
            : base()
        { }

        public CmsDocumentEntity(
           long cmsDocumentId,
           long versionId)
       : base(cmsDocumentId, versionId) { }

        public CmsDocumentEntity(
            long cmsDocumentId,
            long versionId,
            string path)
            : base(cmsDocumentId, versionId)
        {
            Path = path;
        }
        public override string DocumentId => DocumentNature.ToQualifiedStringDocumentId(CmsDocumentId, DocumentNature.Types.Document);

        [JsonProperty("path")]
        public string Path { get; set; }
    }
}