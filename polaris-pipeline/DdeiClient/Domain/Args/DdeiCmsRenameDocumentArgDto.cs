namespace Ddei.Domain.CaseData.Args
{
    public class DdeiCmsRenameDocumentArgDto : DdeiCmsCaseArgDto
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }
    }
}