using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RumpoleGateway.Domain.Validation;

namespace RumpoleGateway.Helpers.Extension
{
    [ExcludeFromCodeCoverage]
    public static class ValidationExtensions
    {
        public static BadRequestObjectResult ToBadRequest<T>(this ValidatableRequest<T> request)
        {
            return new BadRequestObjectResult(request.Errors.Select(e => new
            {
                Field = e.PropertyName,
                Error = e.ErrorMessage
            }));
        }
    }
}
