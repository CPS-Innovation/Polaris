
using Common.Domain.Document;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;

namespace coordinator.Durable.Payloads.Domain
{
    public class PcdRequestEntity : BaseDocumentEntity
    {
        public PcdRequestEntity()
        { }

        public PcdRequestEntity(long cmsDocumentId, PcdRequestDto pcdRequest)
            : base(cmsDocumentId, 1, pcdRequest.PresentationFlags)
        {
            PcdRequest = pcdRequest;
        }

        public override string DocumentId
        {
            get => $"{DocumentNature.PreChargeDecisionRequestPrefix}-{CmsDocumentId}";
        }

        public PcdRequestDto PcdRequest { get; set; }

        public string PresentationTitle
        {
            get => DocumentId;
        }

        public string CmsOriginalFileName
        {
            get => $"{DocumentId}.pdf";
        }

        public string CmsFileCreatedDate
        {
            get => PcdRequest.DecisionRequested;
        }

        public DocumentTypeDto CmsDocType { get; } = new DocumentTypeDto("PCD", null, "Review");
    }
}