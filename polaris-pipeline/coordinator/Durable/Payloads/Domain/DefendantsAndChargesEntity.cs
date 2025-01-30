using Common.Domain.Document;
using Common.Dto.Response.Case;

namespace coordinator.Durable.Payloads.Domain
{
    public class DefendantsAndChargesEntity : BaseDocumentEntity
    {
        public DefendantsAndChargesEntity()
        { }

        public DefendantsAndChargesEntity(long cmsDocumentId, long versionId, DefendantsAndChargesListCoreDto defendantsAndCharges)
            : base(cmsDocumentId, versionId)
        {
            HasMultipleDefendants = defendantsAndCharges?.DefendantCount > 1;
        }

        public override string DocumentId => DocumentNature.ToQualifiedStringDocumentId(CmsDocumentId, DocumentNature.Types.DefendantsAndCharges);

        public bool HasMultipleDefendants { get; set; }
    }
}