using Ddei.Domain.CaseData.Args.Core;
using Common.Dto.Request;

namespace Ddei.Domain.CaseData.Args
{
    public class DdeiReclassifyDocumentArgDto : DdeiDocumentArgDto
    {
        public int DocumentTypeId { get; set; }
        public ReclassificationStatement Statement { get; set; }
        public ReclassificationExhibit Exhibit { get; set; }
        public ReclassificationOther Other { get; set; }
        public ReclassificationImmediate Immediate { get; set; }
    }
}