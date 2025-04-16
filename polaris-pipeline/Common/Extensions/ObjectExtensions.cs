using System;

namespace Common.Extensions;

public static class ObjectExtensions
{
    public static T ExceptionIfNull<T>(this T obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));
        return obj;
    }
}