// <copyright file="AttachmentsResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces.Model;

using System.Text.Json.Serialization;
using System.Collections.Generic;

/// <summary>
/// Represents the response received after a get attachments operation.
/// </summary>
public class AttachmentsResponse
{
    /// <summary>
    /// Gets or sets the list of attachments associated with the response.
    /// Each exhibit contains detailed information about a specific case-related document.
    /// </summary>
    /// <value>A list of <see cref="Attachment"/> objects that represent the attachments.</value>
    [JsonPropertyName("attachments")]
    public List<Attachment>? Attachments { get; set; }
}
