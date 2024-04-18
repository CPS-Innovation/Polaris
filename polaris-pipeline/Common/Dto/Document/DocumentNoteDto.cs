namespace Common.Dto.Document
{
    public class DocumentNoteDto
    {
        public int Id { get; set; }
        public string CreatedByName { get; set; }
        public int SortOrder { get; set; }
        public string Date { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
    }
}