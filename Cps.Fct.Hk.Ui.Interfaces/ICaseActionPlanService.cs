// <copyright file="ICaseActionPlanService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using Cps.Fct.Hk.Ui.Interfaces.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

/// <summary>
/// Interface for case action plan service that provides methods to create and send case action plans.
/// </summary>
public interface ICaseActionPlanService
{
    /// <summary>
    /// Retrieves high-level display information for a specific case.
    /// </summary>
    /// <param name="urn">The urn of the case for which an action plan is being created.</param>
    /// <param name="caseId">The unique identifier of the case for which an action plan is being created.</param>
    /// <param name="addCaseActionPlanRequest">The action plan to create.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve communications.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only <see cref="NoContentResult"/> object.</returns>
    Task<NoContentResult> AddCaseActionPlanAsync(string urn, int caseId, AddCaseActionPlanRequest addCaseActionPlanRequest, CmsAuthValues cmsAuthValues);
}
