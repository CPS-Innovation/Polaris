using System.Linq;
using Common.Constants;
using Common.Dto.Response.Case;
using Common.Dto.Response.Document.FeatureFlags;

namespace coordinator.Durable.Payloads.Domain
{
    public class DefendantsAndChargesEntity : BaseDocumentEntity
    {
        public DefendantsAndChargesEntity()
        { }

        public DefendantsAndChargesEntity(
            long cmsDocumentId,
            long versionId,
            PresentationFlagsDto presentationFlags)
        : base(cmsDocumentId, versionId, presentationFlags) { }

        public DefendantsAndChargesEntity(long cmsDocumentId, DefendantsAndChargesListDto defendantsAndCharges)
            : base(cmsDocumentId, 1, defendantsAndCharges.PresentationFlags)
        {
            DefendantsAndCharges = defendantsAndCharges;
        }
        public override string DocumentId
        {
            get
            {
                return $"{PolarisDocumentTypePrefixes.DefendantsAndCharges}-{CmsDocumentId}";
            }
        }

        public DefendantsAndChargesListDto DefendantsAndCharges { get; set; }

        public bool HasMultipleDefendants => DefendantsAndCharges != null && DefendantsAndCharges.DefendantsAndCharges.Count() > 1;
    }
}