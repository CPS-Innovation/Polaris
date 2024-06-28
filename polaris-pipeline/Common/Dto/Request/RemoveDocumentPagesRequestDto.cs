namespace Common.Dto.Request
{
    public class RemoveDocumentPagesRequestDto
    {
        public int[] PagesIndexesToRemove { get; set; }
        public string FileName { get; set; }
        public string VersionId { get; set; }
    }
}