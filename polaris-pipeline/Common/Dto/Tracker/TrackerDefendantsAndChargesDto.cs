using Common.Dto.Case;
using Common.ValueObjects;
using System;

namespace Common.Dto.Tracker
{
    public class TrackerDefendantsAndChargesDto : BaseTrackerDocumentDto
    {
        public TrackerDefendantsAndChargesDto() 
        { }

        public TrackerDefendantsAndChargesDto(PolarisDocumentId polarisDocumentId, int polarisDocumentVersionId, DefendantsAndChargesListDto defendantsAndCharges)
            : base(polarisDocumentId, polarisDocumentVersionId, $"DAC", 1, defendantsAndCharges.PresentationFlags)
        {
            DefendantsAndCharges = defendantsAndCharges;
        }

        public DefendantsAndChargesListDto DefendantsAndCharges { get; set; }
    }
}