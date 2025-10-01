// <copyright file="VerifyMsResult.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// Represents the result of the VerifyMs operation.
/// </summary>
public class VerifyMsResult
{
    /// <summary>
    /// Gets or sets the status of the VerifyMs operation.
    /// </summary>
    public VerifyMsResultStatus Status { get; set; }

    /// <summary>
    /// Gets or sets an optional message describing the result of the VerifyMs operation.
    /// </summary>
    public string? Message { get; set; }
}
