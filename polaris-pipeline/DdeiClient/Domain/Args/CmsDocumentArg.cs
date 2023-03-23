namespace Ddei.Domain.CaseData.Args
{
    public class CmsDocumentArg : CmsCaseArg
    {
        public string CmsDocCategory { get; set; }

        public int DocumentId { get; set; }

        public long VersionId { get; set; }
    }
}