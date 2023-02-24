using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace PolarisGateway.Domain.Validation;

public class ValidateRequestResult
{
    public Guid CurrentCorrelationId { get; set; }

    public string CmsAuthValues { get; set; }

    public IActionResult InvalidResponseResult { get; set; }

    public StringValues AccessTokenValue { get; set; }
}
