// <copyright file="UpdateStatementResponse.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping;

using System.Text.Json.Serialization;

/// <summary>
/// Represents the response received after update exhibit request.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="UpdateStatementResponse"/> class.
/// </remarks>
/// <param name="updateStatementData">The details of the updated exhibit. Can be null if no material is returned.</param>
public class UpdateStatementResponse(UpdateStatementData updateStatementData)
{
    /// <summary>
    /// Gets or sets the details of the updated material.
    /// </summary>
    [JsonPropertyName("updateStatement")]
    public UpdateStatementData UpdateStatementData { get; set; } = updateStatementData;
}

/// <summary>
/// Represents the details of the updated material response data.
/// </summary>
public class UpdateStatementData
{
    /// <summary>
    /// Gets or sets the material id of the updated material.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
