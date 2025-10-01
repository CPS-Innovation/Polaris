// <copyright file="InitResultStatus.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// Represents the result status of the Init operation.
/// </summary>
public enum InitResultStatus
{
    /// <summary>
    /// Indicates that the request was bad or invalid.
    /// </summary>
    BadRequest,

    /// <summary>
    /// Indicates that a redirect is required for the response.
    /// </summary>
    Redirect,

    /// <summary>
    /// Indicates that an internal server error occurred.
    /// This is typically used when there is a misconfiguration or an unexpected condition.
    /// </summary>
    ServerError,
}
