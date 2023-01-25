using System;

namespace PolarisGateway.Domain.Validation;

public class ValidateRequestResult : ValidateAuthRequestResult
{
    public Guid CurrentCorrelationId { get; set; }

    public string UpstreamToken { get; set; }
}
