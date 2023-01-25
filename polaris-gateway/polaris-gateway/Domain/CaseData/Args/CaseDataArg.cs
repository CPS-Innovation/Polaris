using System;

namespace PolarisGateway.Domain.CaseData.Args
{
    public class CaseDataArg
    {
        public string OnBehalfOfToken { get; set; }
        public string UpstreamToken { get; set; }
        public Guid CorrelationId { get; set; }
    }
}