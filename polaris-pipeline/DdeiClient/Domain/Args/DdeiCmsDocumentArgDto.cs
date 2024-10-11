namespace Ddei.Domain.CaseData.Args
{
    public class DdeiCmsDocumentArgDto : DdeiCmsCaseArgDto
    {
        public long DocumentId { get; set; }

        public long VersionId { get; set; }
    }
}