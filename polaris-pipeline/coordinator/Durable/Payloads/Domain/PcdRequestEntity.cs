using Common.Constants;
using Common.Dto.Case.PreCharge;
using Common.Dto.FeatureFlags;

namespace coordinator.Durable.Payloads.Domain
{
    public class PcdRequestEntity : BaseDocumentEntity
    {
        public PcdRequestEntity()
        { }

        public PcdRequestEntity(string documentId,
            long versionId,
            PresentationFlagsDto presentationFlags)
        : base(documentId, versionId, presentationFlags) { }

        public PcdRequestEntity(string cmsDocumentId, PcdRequestDto pcdRequest)
            : base($"{PolarisDocumentTypePrefixes.PcdRequest}-{cmsDocumentId}", 1, pcdRequest.PresentationFlags)
        {
            PcdRequest = pcdRequest;
        }

        public PcdRequestDto PcdRequest { get; set; }
    }
}