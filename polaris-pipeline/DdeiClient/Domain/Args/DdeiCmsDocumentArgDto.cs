namespace Ddei.Domain.CaseData.Args
{
    public class DdeiCmsDocumentArgDto : DdeiCmsCaseArgDto
    {
        public int DocumentId { get; set; }

        public long VersionId { get; set; }
    }
}