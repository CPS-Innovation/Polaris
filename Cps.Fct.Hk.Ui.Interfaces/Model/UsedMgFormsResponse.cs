// <copyright file="UsedMgFormsResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;
using System.Collections.Generic;

/// <summary>
/// Represents the response received after a get used MG forms operation.
/// </summary>
public class UsedMgFormsResponse
{
    /// <summary>
    /// Gets or sets the list of used MG forms associated with the response.
    /// Each MG form contains detailed information about a specific case-related document.
    /// </summary>
    /// <value>A list of <see cref="MgForm"/> objects that represent the used MG forms.</value>
    [JsonPropertyName("mgForms")]
    public List<MgForm>? MgForms { get; set; }
}
