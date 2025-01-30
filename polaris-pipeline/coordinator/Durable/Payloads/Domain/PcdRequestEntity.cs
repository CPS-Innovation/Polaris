
using Common.Domain.Document;

namespace coordinator.Durable.Payloads.Domain
{
    public class PcdRequestEntity : BaseDocumentEntity
    {
        public PcdRequestEntity()
        { }

        public PcdRequestEntity(long cmsDocumentId, long versionId)
            : base(cmsDocumentId, versionId)
        { }

        public override string DocumentId => DocumentNature.ToQualifiedStringDocumentId(CmsDocumentId, DocumentNature.Types.PreChargeDecisionRequest);
    }
}