// <copyright file="GetExhibitProducersRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;
using System;

/// <summary>
/// The request to get the exhibit producers request.
/// </summary>
/// <param name="CaseId">The case ID.</param>
/// <param name="CorrespondenceId">The correspondence ID.</param>
public record GetExhibitProducersRequest(int CaseId, Guid CorrespondenceId)
    : BaseRequest(CorrespondenceId: CorrespondenceId)
{
}
