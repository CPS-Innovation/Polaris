namespace Ddei.Domain.CaseData.Args
{
    public class DdeiCmsDocumentArgDto : DdeiCmsCaseArgDto
    {
        public string CmsDocCategory { get; set; }

        public int DocumentId { get; set; }

        public long VersionId { get; set; }
    }
}