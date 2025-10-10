// <copyright file="MatchedCommunication.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// Represents a request for a matched communication.
/// </summary>
public class MatchedCommunication
{
#pragma warning disable SA1300 // Element should begin with upper-case letter

    /// <summary>
    /// Gets or sets the material ID associated with the communication.
    /// </summary>
    public int materialId { get; set; }

    /// <summary>
    /// Gets or sets the subject of the communication.
    /// </summary>
    public string subject { get; set; } = string.Empty;

#pragma warning restore SA1300 // Element should begin with upper-case letter
}
