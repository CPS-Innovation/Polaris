namespace Ddei.Domain.Response.Document
{
    public class DocumentNoteResponse
    {
        public int Id { get; set; }
        public string CreatedByName { get; set; }
        public int SequenceNumber { get; set; }
        public string Date { get; set; }
        public string Text { get; set; }
        public string CmsNoteType { get; set; }
    }
}