using System;

namespace Common.Extensions;

public static class ObjectExtensions
{
    public static T ExceptionIfNull<T>(this T obj) where T : class
    {
        return obj ?? throw new ArgumentNullException(nameof(obj));
    }
}