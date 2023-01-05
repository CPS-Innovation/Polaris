using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace PolarisGateway.Domain.Validation;

public class ValidateRequestResult
{
    public IActionResult InvalidResponseResult { get; set; }

    public Guid CurrentCorrelationId { get; set; }

    public StringValues AccessTokenValue { get; set; }

    public string UpstreamToken { get; set; }
}
