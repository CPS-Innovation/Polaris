using System.ComponentModel.DataAnnotations;

namespace Common.Validators;

//Created to get around AutoFixture's issues with Range and Long
public class RequiredLongGreaterThanZeroAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        return value != null && long.TryParse(value.ToString(), out var validValue) && validValue > 0;
    }
}
