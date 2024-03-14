using System;

namespace coordinator.Clients.Ddei.Domain.CaseData.Args
{
    public class DdeiBaseArg
    {
        public string CmsAuthValues { get; set; }
        public Guid CorrelationId { get; set; }
    }
}