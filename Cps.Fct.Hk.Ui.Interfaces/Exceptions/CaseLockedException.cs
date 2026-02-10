// <copyright file="CaseLockedException.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a request cannot be processed due to Case being locked state.
/// </summary>
public class CaseLockedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CaseLockedException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CaseLockedException(string message)
        : base(message)
    {
    }
}
