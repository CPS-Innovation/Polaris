using FluentValidation.Results;

namespace polaris_common.Domain.Extensions
{
    public static class ValidationResultExtensions
    {
        public static string FlattenErrors(this ValidationResult validationResult)
        {
            var errorsThrown = validationResult.Errors.Select(e => new
            {
                Field = e.PropertyName,
                Error = e.ErrorMessage
            });

            return string.Join(Environment.NewLine, errorsThrown);
        }
    }
}
