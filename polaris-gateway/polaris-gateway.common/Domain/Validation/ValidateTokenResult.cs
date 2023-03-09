using System.Collections.Generic;
using FluentValidation.Results;

namespace PolarisGateway.Domain.Validation
{
    public class ValidateTokenResult
    {
        public bool IsValid { get; set; }

        public string UserName { get; set; }
    }
}
