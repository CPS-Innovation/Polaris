namespace Ddei.Domain.CaseData.Args
{
    public class DdeiCmsAddDocumentNoteArgDto : DdeiCmsCaseDataArgDto
    {
        public string Urn { get; set; }
        public int CaseId { get; set; }
        public int DocumentId { get; set; }
        public string Text { get; set; }
    }
}