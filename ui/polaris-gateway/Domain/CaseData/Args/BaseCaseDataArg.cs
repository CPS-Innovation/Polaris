using System;

namespace PolarisGateway.Domain.CaseData.Args
{
    public abstract class BaseCaseDataArg
    {
        public string OnBehalfOfToken { get; set; }
        public string UpstreamToken { get; set; }
        public Guid CorrelationId { get; set; }
    }
}