// <copyright file="GetPcdRequestsCoreRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>
namespace Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model.Requests.PcdRequests;
using System;

/// <summary>
/// Get PCD requests overview data request by case id.
/// </summary>
/// <param name="caseId"></param>
/// <param name="CorrespondenceId"></param>
public record GetPcdRequestsOverviewRequest(int caseId, Guid CorrespondenceId)
        : BaseRequest(CorrespondenceId: CorrespondenceId)
{
}
