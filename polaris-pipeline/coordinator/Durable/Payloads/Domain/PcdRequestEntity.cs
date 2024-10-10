using Common.Dto.Case.PreCharge;

namespace coordinator.Durable.Payloads.Domain
{
    public class PcdRequestEntity : BaseDocumentEntity
    {
        public PcdRequestEntity()
        { }

        public PcdRequestEntity(string documentId, PcdRequestDto pcdRequest)
            : base(documentId, 1, pcdRequest.PresentationFlags)
        {
            PcdRequest = pcdRequest;
        }

        public PcdRequestDto PcdRequest { get; set; }
    }
}