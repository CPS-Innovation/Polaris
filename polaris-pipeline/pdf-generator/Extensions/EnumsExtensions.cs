using System;
using pdf_generator.Attributes;

namespace pdf_generator.Extensions;

public static class EnumsExtensions
{
    public static string GetEnumValue(this Enum value)
    {
        var type = value.GetType();
        var fieldInfo = type.GetField(value.ToString());

        // Return the first if there was a match
        return fieldInfo != null && fieldInfo.GetCustomAttributes(
            typeof(EnumValueAttribute), false) is EnumValueAttribute[] { Length: > 0 } attribs
            ? attribs[0].EnumValue : null;
    }
}