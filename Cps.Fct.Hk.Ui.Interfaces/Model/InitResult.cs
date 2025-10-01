// <copyright file="InitResult.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// Represents the result of the Init operation.
/// </summary>
public class InitResult
{
    /// <summary>
    /// Gets or sets the status of the Init operation.
    /// </summary>
    public InitResultStatus Status { get; set; }

    /// <summary>
    /// Gets or sets an optional message describing the result of the Init operation.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets an optional URL to redirect the client to after the Init operation.
    /// </summary>
    public string? RedirectUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a cookie should be set in the response.
    /// </summary>
    /// <value>
    /// A boolean value indicating whether a cookie should be set.
    /// If <c>true</c>, a cookie should be set; otherwise, no cookie will be set.
    /// </value>
    public bool ShouldSetCookie { get; set; }

    /// <summary>
    /// Gets or sets the case ID used in the Init operation.
    /// </summary>
    /// <value>
    /// The case ID used in the Init operation. This may be <c>null</c> if not applicable.
    /// </value>
    public string? CaseId { get; set; }

    /// <summary>
    /// Gets or sets the CMS session cookie value used in the Init operation.
    /// </summary>
    /// <value>
    /// The CMS session cookie value. This may be <c>null</c> if not applicable.
    /// </value>
    public string? Cc { get; set; }

    /// <summary>
    /// Gets or sets the CMS access token used in the Init operation.
    /// </summary>
    /// <value>
    /// The CMS access token. This may be <c>null</c> if not applicable.
    /// </value>
    public string? Ct { get; set; }
}
