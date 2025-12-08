// <copyright file="CaseLockedStatusResult.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the response received after a get used MG forms operation.
/// </summary>
public class CaseLockedStatusResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the case is locked or not.
    /// </summary>
    [JsonPropertyName("isLocked")]
    public bool IsLocked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating locked by user value.
    /// </summary>
    [JsonPropertyName("lockedByUser")]
    public string LockedByUser { get; set; }

    /// <summary>
    /// Gets or sets a value indicating case locked message value.
    /// </summary>
    [JsonPropertyName("caseLockedMessage")]
    public string CaseLockedMessage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether case locked by current user value.
    /// </summary>
    [JsonPropertyName("isLockedByCurrentUser")]
    public bool IsLockedByCurrentUser { get; set; }
}
