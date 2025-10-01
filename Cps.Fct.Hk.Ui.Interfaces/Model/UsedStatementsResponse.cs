// <copyright file="UsedStatementsResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;
using System.Collections.Generic;

/// <summary>
/// Represents the response received after a get used statements operation.
/// </summary>
public class UsedStatementsResponse
{
    /// <summary>
    /// Gets or sets the list of used statements associated with the response.
    /// Each statement contains detailed information about a specific case-related document.
    /// </summary>
    /// <value>A list of <see cref="Statement"/> objects that represent the used statements.</value>
    [JsonPropertyName("statements")]
    public List<Statement>? Statements { get; set; }
}
