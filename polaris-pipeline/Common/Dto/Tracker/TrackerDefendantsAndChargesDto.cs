using Common.Dto.Case;
using System;

namespace Common.Dto.Tracker
{
    public class TrackerDefendantsAndChargesDto : BaseTrackerDocumentDto
    {
        public TrackerDefendantsAndChargesDto() 
        { }

        public TrackerDefendantsAndChargesDto(Guid polarisDocumentId, int polarisDocumentVersionId, DefendantsAndChargesListDto defendantsAndCharges)
            : base(polarisDocumentId, polarisDocumentVersionId, $"DAC-{defendantsAndCharges.CaseId}", 1, defendantsAndCharges.PresentationFlags)
        {
            DefendantsAndCharges = defendantsAndCharges;
        }

        public DefendantsAndChargesListDto DefendantsAndCharges { get; set; }
    }
}