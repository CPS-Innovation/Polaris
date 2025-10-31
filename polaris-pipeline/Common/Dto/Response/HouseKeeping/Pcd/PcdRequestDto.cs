// <copyright file="PcdRequestDto.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Response.HouseKeeping.Pcd;

using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// PCD Request.
/// </summary>
public class PcdRequestDto : PcdRequestCore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PcdRequestDto"/> class.
    /// </summary>
    public PcdRequestDto()
    {
        CaseOutline = new List<PcdCaseOutlineLine>();
        Suspects = new List<PcdRequestSuspect>();
        PoliceContactDetails = new List<PCDPoliceContactDetails>();
        MaterialProvided = new List<MaterialProvided>();
    }

    /// <summary>
    /// Gets or sets the Pcd request caseOutline.
    /// </summary>
    [JsonPropertyName("caseOutline")]
    public List<PcdCaseOutlineLine>? CaseOutline { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request comments.
    /// </summary>
    [JsonPropertyName("comments")]
    public PcdComments Comments { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request suspects.
    /// </summary>
    [JsonPropertyName("suspects")]
    public List<PcdRequestSuspect>? Suspects { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request policeContactDetails.
    /// </summary>
    [JsonPropertyName("policeContactDetails")]
    public List<PCDPoliceContactDetails>? PoliceContactDetails { get; set; }

    /// <summary>
    /// Gets or sets the Pcd request materialProvided.
    /// </summary>
    [JsonPropertyName("materialProvided")]
    public List<MaterialProvided>? MaterialProvided { get; set; }
}
