using polaris_common.Dto.Case.PreCharge;
using polaris_common.ValueObjects;

namespace polaris_common.Domain.Entity
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