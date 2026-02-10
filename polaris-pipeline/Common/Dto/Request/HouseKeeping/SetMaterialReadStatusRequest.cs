// <copyright file="SetMaterialReadStatusRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;

using System;
using System.Text.Json.Serialization;
using Common.Enums;

/// <summary>
/// Represents a request to set a material read status.
/// </summary>
/// <param name="id"></param>
/// <param name="materialId">Material id to be updated.</param>
/// <param name="state">Enum Invalid = 0, Read = 1, Unread = 2</param>
public record SetMaterialReadStatusRequest(
    Guid CorrespondenceId,
    [property: JsonPropertyName("materialId")] int materialId,
    [property: JsonPropertyName("state")] MaterialReadStatusType state)
    : BaseRequest(CorrespondenceId: CorrespondenceId)
{
}
