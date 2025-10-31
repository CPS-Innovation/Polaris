// <copyright file="GetWitnessesForCaseRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;
using System;

/// <summary>
/// The request to get the case summary.
/// </summary>
/// <param name="CaseId">The numeric case ID.</param>
/// <param name="CorrespondenceId">The correspondence ID.</param>
public record GetCaseWitnessesRequest(int CaseId, Guid CorresponenceId)
    : BaseRequest(CorrespondenceId: CorresponenceId)
{
}
