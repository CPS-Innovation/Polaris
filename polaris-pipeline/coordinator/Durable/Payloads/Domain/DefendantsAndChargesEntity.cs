using Common.Dto.Case;
using Common.ValueObjects;

namespace coordinator.Durable.Payloads.Domain
{
    public class DefendantsAndChargesEntity : BaseDocumentEntity
    {
        public DefendantsAndChargesEntity()
        { }

        public DefendantsAndChargesEntity(PolarisDocumentId polarisDocumentId, int polarisDocumentVersionId, DefendantsAndChargesListDto defendantsAndCharges)
            : base(polarisDocumentId, polarisDocumentVersionId, $"DAC", 1, defendantsAndCharges.PresentationFlags)
        {
            DefendantsAndCharges = defendantsAndCharges;
        }

        public DefendantsAndChargesListDto DefendantsAndCharges { get; set; }
    }
}