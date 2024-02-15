using Microsoft.AspNetCore.Mvc;

namespace PolarisGateway.Domain.Validation;

public class ValidateRequestResult
{
    public string CmsAuthValues { get; set; }

    public IActionResult InvalidResponseResult { get; set; }

}
