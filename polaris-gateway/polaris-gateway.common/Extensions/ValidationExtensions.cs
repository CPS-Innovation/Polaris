using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PolarisGateway.Domain.Validation;

namespace PolarisGateway.Extensions
{
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
