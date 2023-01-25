using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace PolarisGateway.Domain.Validation;

public class ValidateAuthRequestResult
{
    public IActionResult InvalidResponseResult { get; set; }

    public StringValues AccessTokenValue { get; set; }

}
