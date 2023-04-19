using Common.Dto.FeatureFlags;
using System;

namespace Common.Dto.Tracker
{
    public class TrackerPcdRequestDto : BaseTrackerDocumentDto
    {
        public TrackerPcdRequestDto(Guid polarisDocumentId, int polarisDocumentVersionId, int pcdRequestId, PresentationFlagsDto presentationFlags)
            : base(polarisDocumentId, polarisDocumentVersionId, $"PCD-{pcdRequestId}", 1, presentationFlags)
        {
            PcdRequestId = pcdRequestId;
        }

        public int PcdRequestId { get; set; }
    }
}