// <copyright file="GetUsedOtherMaterialsRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;

using System;

/// <summary>
/// The request to get the used other materials for a case.
/// </summary>
/// <param name="CaseId">The numeric case ID.</param>
/// <param name="CorrespondenceId">The correspondence ID.</param>
public record GetUsedOtherMaterialsRequest(int CaseId, Guid CorrespondenceId)
        : BaseRequest(CorrespondenceId: CorrespondenceId)
{
}

