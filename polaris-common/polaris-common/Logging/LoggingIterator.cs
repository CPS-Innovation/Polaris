﻿namespace polaris_common.Logging;

public static class LoggingIterator
{
    private static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem, Func<TSource, bool> canContinue)
    {
        for (var current = source; canContinue(current); current = nextItem(current))
        {
            yield return current;
        }
    }

    public static IEnumerable<TSource> FromHierarchy<TSource>(this TSource source, Func<TSource, TSource> nextItem)
        where TSource : class
    {
        return FromHierarchy(source, nextItem, s => s != null);
    }
}
