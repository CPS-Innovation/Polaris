// <copyright file="GetWitnessesForCaseRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;
using System;

/// <summary>
/// The request to get the case summary.
/// </summary>
/// <param name="CaseId">The numeric case ID.</param>
/// <param name="CorrespondenceId">The correspondence ID.</param>
public record GetWitnessesForCaseRequest(int CaseId, Guid CorresponenceId)
    : BaseRequest(CorrespondenceId: CorresponenceId)
{
}
