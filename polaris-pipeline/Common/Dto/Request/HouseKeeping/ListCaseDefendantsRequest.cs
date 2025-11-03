// <copyright file="ListCaseDefendantsRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;

using System;

/// <summary>
/// The request to list case defendants.
/// </summary>
/// <param name="CaseId">The numeric case ID.</param>
/// <param name="CorrespondenceId">The correspondence ID.</param>
public record ListCaseDefendantsRequest(int CaseId, Guid CorrespondenceId)
        : BaseRequest(CorrespondenceId: CorrespondenceId)
{
}

