// <copyright file="BulkSetUnusedResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents the response received after a bulk reclassification operation.
/// </summary>
public class BulkSetUnusedResponse
{
    /// <summary>
    /// Gets or sets the status of the bulk operation.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the message related to the operation result.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the list of successfully reclassified materials.
    /// </summary>
    [JsonPropertyName("reclassifiedMaterials")]
    public List<ReclassifiedMaterial> ReclassifiedMaterials { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of materials that failed to be reclassified.
    /// </summary>
    [JsonPropertyName("failedMaterials")]
    public List<FailedMaterial> FailedMaterials { get; set; } = new();
}

/// <summary>
/// Represents a material that has been successfully reclassified.
/// </summary>
public class ReclassifiedMaterial
{
    /// <summary>
    /// Gets or sets the material ID that was successfully reclassified.
    /// </summary>
    [JsonPropertyName("materialId")]
    public int MaterialId { get; set; }

    /// <summary>
    /// Gets or sets the subject of the material.
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; }
}

/// <summary>
/// Represents a material that failed to be reclassified.
/// </summary>
public class FailedMaterial
{
    /// <summary>
    /// Gets or sets the material ID that failed to be reclassified.
    /// </summary>
    [JsonPropertyName("materialId")]
    public int MaterialId { get; set; }

    /// <summary>
    /// Gets or sets the subject of the material.
    /// </summary>
    [JsonPropertyName("subject")]
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the error message associated with the failure.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; }
}
