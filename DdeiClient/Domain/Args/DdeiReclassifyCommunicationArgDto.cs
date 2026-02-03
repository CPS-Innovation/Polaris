using Ddei.Domain.CaseData.Args.Core;
using Common.Dto.Request;

namespace Ddei.Domain.CaseData.Args
{
    public class DdeiReclassifyCommunicationArgDto : MdsDocumentArgDto
    {
        public string Classification { get; set; }
        public long MaterialId { get; set; }
        public int DocumentTypeId { get; set; }
        public ReclassificationExhibit Exhibit { get; set; }
        public ReclassificationStatement Statement { get; set; }
        public bool? Used { get; set; }
        public string Subject { get; set; }
    }
}