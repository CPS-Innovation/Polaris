using Common.Constants;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document.FeatureFlags;

namespace coordinator.Durable.Payloads.Domain
{
    public class PcdRequestEntity : BaseDocumentEntity
    {
        public PcdRequestEntity()
        { }

        public PcdRequestEntity(
            long cmsDocumentId,
            long versionId,
            PresentationFlagsDto presentationFlags)
        : base(cmsDocumentId, versionId, presentationFlags) { }

        public PcdRequestEntity(long cmsDocumentId, PcdRequestDto pcdRequest)
            : base(cmsDocumentId, 1, pcdRequest.PresentationFlags)
        {
            PcdRequest = pcdRequest;
        }

        public override string DocumentId
        {
            get
            {
                return $"{PolarisDocumentTypePrefixes.PcdRequest}-{CmsDocumentId}";
            }
        }

        public PcdRequestDto PcdRequest { get; set; }
    }
}