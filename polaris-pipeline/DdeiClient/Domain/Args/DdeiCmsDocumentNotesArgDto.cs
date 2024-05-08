namespace Ddei.Domain.CaseData.Args
{
    public class DdeiCmsDocumentNotesArgDto : DdeiCmsCaseDataArgDto
    {
        public string Urn { get; set; }
        public int CaseId { get; set; }
        public string DocumentId { get; set; }
    }
}