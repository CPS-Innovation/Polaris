using Common.Dto.Case;

namespace Ddei.Domain.CaseData.Args
{
    public class DdeiCmsReorderStatementsArgDto : DdeiCmsCaseDataArgDto
    {
        public string Urn { get; set; }
        public int CaseId { get; set; }
        public OrderedStatementsDto OrderedStatements { get; set; }
    }
}