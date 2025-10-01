// <copyright file="ExceptionExtensions.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Diagnostics;

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
    public static string ToAggregatedMessage(this Exception? exception)
    {
        //Requires.NotNull(exception);

        if (exception == null)
        {
            return string.Empty;
        }

        return GetAggregatedMessage(exception);
    }

    private static string GetAggregatedMessage(Exception? exception)
    {
        if (exception == null)
        {
            return string.Empty;
        }

        string message = GetAggregatedMessage(exception.InnerException);

        return $"{message}{Environment.NewLine}{exception.Message}".Trim();
    }
}
