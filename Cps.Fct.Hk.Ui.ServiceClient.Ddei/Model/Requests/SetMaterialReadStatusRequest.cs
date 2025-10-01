// <copyright file="SetMaterialReadStatusRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model.Requests;

using System.Text.Json.Serialization;
using Cps.Fct.Hk.Ui.Interfaces.Enums;

/// <summary>
/// Represents a request to set a material read status.
/// </summary>
/// <param name="id"></param>
/// <param name="materialId">Material id to be updated.</param>
/// <param name="state">Enum Invalid = 0, Read = 1, Unread = 2</param>
public record SetMaterialReadStatusRequest(
    Guid CorrespondenceId,
    [property: JsonPropertyName("materialId")] int materialId,
    [property: JsonPropertyName("state")] SetMaterialReadStatusType state)
    : BaseRequest(CorrespondenceId: CorrespondenceId)
{
}
