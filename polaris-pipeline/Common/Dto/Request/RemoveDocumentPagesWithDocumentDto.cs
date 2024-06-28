namespace Common.Dto.Request
{
    public class RemoveDocumentPagesWithDocumentDto
    {
        public int[] PagesIndexesToRemove { get; set; }
        public string Document { get; set; }
        public string FileName { get; set; }
        public string VersionId { get; set; }
    }
}