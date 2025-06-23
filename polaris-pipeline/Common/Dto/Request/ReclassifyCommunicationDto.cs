#nullable enable

namespace Common.Dto.Request
{
    public class ReclassifyCommunicationDto
    {
        public string? Classification { get; set; }
        public int MaterialId { get; set; }
        public int DocumentTypeId { get; set; }
        public CommunicationExhibitType? Exhibit { get; set; }
        public CommunicationStatementType? Statement { get; set; }
        public bool? Used { get; set; }
        public string? Subject { get; set; }
    }
}