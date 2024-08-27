namespace Common.Dto.Request
{
    public class DocumentReclassificationRequestDto
    {
        public DocumentReclassificationRequestDto()
        {
            ReclassificationType = ReclassificationType.Other;
        }

        public int DocumentId { get; set; }
        public int DocumentTypeId { get; set; }
        public ReclassificationStatement Statement { get; set; }
        public ReclassificationExhibit Exhibit { get; set; }
        public ReclassificationType ReclassificationType { get; set; }
        public bool Used { get; set; }
    }

    public enum ReclassificationType
    {
        Exhibit,
        Statement,
        Other
    }
}