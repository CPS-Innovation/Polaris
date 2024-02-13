using Common.Dto.Case;
using Common.ValueObjects;
using Common.Domain.Entity;

namespace coordinator.Domain.Entity
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