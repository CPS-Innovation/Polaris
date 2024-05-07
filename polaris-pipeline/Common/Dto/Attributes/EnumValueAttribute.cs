using System;

namespace Common.Dto.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class EnumValueAttribute : Attribute
{
    public string EnumValue { get; protected set; }

    public EnumValueAttribute(string value)
    {
        EnumValue = value;
    }
}