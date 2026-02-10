using Ddei.Domain.CaseData.Args.Core;

namespace Ddei.Domain.CaseData.Args
{
    public class MdsDocumentIdAndVersionIdArgDto : MdsDocumentArgDto
    {
        public long VersionId { get; set; }
    }
}