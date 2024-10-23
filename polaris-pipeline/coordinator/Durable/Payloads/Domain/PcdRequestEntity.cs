
using Common.Domain.Document;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;

namespace coordinator.Durable.Payloads.Domain
{
    public class PcdRequestEntity : BaseDocumentEntity
    {
        public PcdRequestEntity()
        { }

        public PcdRequestEntity(long cmsDocumentId, long versionId, PcdRequestCoreDto pcdRequest)
            : base(cmsDocumentId, versionId, pcdRequest.PresentationFlags)
        {
            CmsFileCreatedDate = pcdRequest.DecisionRequested;
        }

        public override string DocumentId
        {
            get => $"{DocumentNature.PreChargeDecisionRequestPrefix}-{CmsDocumentId}";
        }

        public string PresentationTitle
        {
            get => DocumentId;
        }

        public string CmsOriginalFileName
        {
            get => $"{DocumentId}.pdf";
        }

        public string CmsFileCreatedDate { get; set; }


        public DocumentTypeDto CmsDocType { get; } = new DocumentTypeDto("PCD", null, "Review");
    }
}