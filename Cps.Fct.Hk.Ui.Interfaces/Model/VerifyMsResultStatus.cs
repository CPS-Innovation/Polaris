// <copyright file="VerifyMsResultStatus.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// Represents the result status of the VerifyMs operation.
/// </summary>
public enum VerifyMsResultStatus
{
    /// <summary>
    /// Indicates that the operation completed successfully without errors.
    /// </summary>
    OK,

    /// <summary>
    /// Indicates that the request was bad or invalid.
    /// </summary>
    BadRequest,

    /// <summary>
    /// Indicates that the request was understood, but the server refuses to authorize it.
    /// Typically corresponds to an HTTP 401 Unauthorized status.
    /// </summary>
    Unauthorized,

    /// <summary>
    /// Indicates that an internal server error occurred.
    /// This is typically used when there is a misconfiguration or an unexpected condition.
    /// </summary>
    ServerError,
}
