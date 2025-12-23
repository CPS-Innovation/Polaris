// <copyright file="BulkSetUnusedRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;

/// <summary>
/// Represents a request for reclassifying a communication.
/// </summary>
public record BulkSetUnusedRequest
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
