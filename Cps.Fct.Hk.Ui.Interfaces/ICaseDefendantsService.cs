// <copyright file="ICaseDefendantsService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using System.Threading.Tasks;
using Common.Dto.Request;
using Common.Dto.Response.HouseKeeping;

/// <summary>
/// Interface for case defendants service that provides methods to retrieve case defendants.
/// </summary>
public interface ICaseDefendantsService
{
    /// <summary>
    /// Service that retrieves defendants for a case.
    /// </summary>
    /// <param name="caseId">Unique identifier of the case to retrieve defendants for.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve defendants.</param>
    /// <returns>A task <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<DefendantsResponse> GetCaseDefendantsAsync(int caseId, CmsAuthValues cmsAuthValues);
}
