using System;

namespace PolarisAuthHandover.Domain.CaseData.Args
{
    public class CaseDataArg
    {
        public string CmsAuthValues { get; set; }
        public Guid CorrelationId { get; set; }
    }
}