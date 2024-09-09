namespace Ddei.Domain.CaseData.Args
{
    public class DdeiCmsWitnessStatementsArgDto : DdeiCmsCaseDataArgDto
    {
        public string Urn { get; set; }
        public int CaseId { get; set; }
        public int WitnessId { get; set; }
    }
}