using Ddei.Domain.CaseData.Args.Core;

namespace Ddei.Domain.CaseData.Args
{
    public class DdeiDocumentNotesArgDto : DdeiBaseArgDto
    {
        public string Urn { get; set; }
        public int CaseId { get; set; }
        public long DocumentId { get; set; }
    }
}