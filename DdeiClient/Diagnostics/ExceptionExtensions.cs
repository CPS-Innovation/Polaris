// <copyright file="ExceptionExtensions.cs" company="TheCrownProsecutionService">
// Copyright (c) TheCrownProsecutionService. All rights reserved.
// </copyright>

namespace DdeiClient.Diagnostics;

using System;

/// <summary>
/// Extension methods for <see cref="Exception"/>.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Returns the aggregated message of the exception and all its inner exceptions.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>An aggregated message of the exception and all its inner exceptions.</returns>
    public static string ToAggregatedMessage(this Exception exception)
    {
        if (exception == null)
        {
            return string.Empty;
        }

        return GetAggregatedMessage(exception);
    }

    private static string GetAggregatedMessage(Exception exception)
    {
        if (exception == null)
        {
            return string.Empty;
        }

        string message = GetAggregatedMessage(exception.InnerException);

        return $"{message}{Environment.NewLine}{exception.Message}".Trim();
    }
}
