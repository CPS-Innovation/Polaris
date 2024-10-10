using System.Linq;
using Common.Constants;
using Common.Dto.Case;
using Common.Dto.FeatureFlags;

namespace coordinator.Durable.Payloads.Domain
{
    public class DefendantsAndChargesEntity : BaseDocumentEntity
    {
        public DefendantsAndChargesEntity()
        { }

        public DefendantsAndChargesEntity(string documentId,
            long versionId,
            PresentationFlagsDto presentationFlags)
        : base(documentId, versionId, presentationFlags) { }

        public DefendantsAndChargesEntity(string cmsDocumentId, DefendantsAndChargesListDto defendantsAndCharges)
            : base($"{PolarisDocumentTypePrefixes.DefendantsAndCharges}-{cmsDocumentId}", 1, defendantsAndCharges.PresentationFlags)
        {
            DefendantsAndCharges = defendantsAndCharges;
        }

        public DefendantsAndChargesListDto DefendantsAndCharges { get; set; }

        public bool HasMultipleDefendants => DefendantsAndCharges != null && DefendantsAndCharges.DefendantsAndCharges.Count() > 1;
    }
}