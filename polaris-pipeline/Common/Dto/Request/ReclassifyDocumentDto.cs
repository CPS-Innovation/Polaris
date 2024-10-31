namespace Common.Dto.Request
{
    public class ReclassifyDocumentDto
    {
        public int DocumentTypeId { get; set; }
        public ReclassificationStatement Statement { get; set; }
        public ReclassificationExhibit Exhibit { get; set; }
        public ReclassificationOther Other { get; set; }
        public ReclassificationImmediate Immediate { get; set; }
    }
}