using Common.Dto.Case.PreCharge;
using Common.ValueObjects;
using System;

namespace Common.Dto.Tracker
{
    public class TrackerPcdRequestDto : BaseTrackerDocumentDto
    {
        public TrackerPcdRequestDto() 
        { }

        public TrackerPcdRequestDto(PolarisDocumentId polarisDocumentId, int polarisDocumentVersionId, PcdRequestDto pcdRequest)
            : base(polarisDocumentId, polarisDocumentVersionId, $"PCD-{pcdRequest.Id}", 1, pcdRequest.PresentationFlags)
        {
            PcdRequest = pcdRequest;
        }

        public PcdRequestDto PcdRequest { get; set; }
    }
}