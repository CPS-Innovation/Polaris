using Common.Dto.Request;

namespace Ddei.Domain.CaseData.Args
{
    public class DdeiCmsReclassifyDocumentArgDto : DdeiCmsCaseArgDto
    {
        public int DocumentId { get; set; }
        public int DocumentTypeId { get; set; }
        public ReclassificationStatement Statement { get; set; }
        public ReclassificationExhibit Exhibit { get; set; }
        public ReclassificationOther Other { get; set; }
        public ReclassificationImmediate Immediate { get; set; }
    }
}