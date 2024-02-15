using Common.Dto.Case.PreCharge;
using Common.ValueObjects;
using Common.Domain.Entity;

namespace coordinator.Domain.Entity
{
    public class PcdRequestEntity : BaseDocumentEntity
    {
        public PcdRequestEntity()
        { }

        public PcdRequestEntity(PolarisDocumentId polarisDocumentId, int polarisDocumentVersionId, PcdRequestDto pcdRequest)
            : base(polarisDocumentId, polarisDocumentVersionId, $"PCD-{pcdRequest.Id}", 1, pcdRequest.PresentationFlags)
        {
            PcdRequest = pcdRequest;
        }

        public PcdRequestDto PcdRequest { get; set; }
    }
}