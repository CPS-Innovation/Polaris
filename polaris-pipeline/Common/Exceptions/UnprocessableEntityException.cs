// <copyright file="UnprocessableEntityException.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

using System;

namespace Common.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a request cannot be processed due to invalid data or constraints.
/// </summary>
public class UnprocessableEntityException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnprocessableEntityException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public UnprocessableEntityException(string message)
        : base(message)
    {
    }
}
