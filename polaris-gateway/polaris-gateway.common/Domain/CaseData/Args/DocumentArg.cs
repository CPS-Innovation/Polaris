namespace PolarisGateway.Domain.CaseData.Args
{
    public class DocumentArg : CaseArg
    {
        // todo: is this ok to be tied to the business domain CmsDocCategory
        public CmsDocCategory CmsDocCategory { get; set; }
        public int DocumentId { get; set; }
    }
}