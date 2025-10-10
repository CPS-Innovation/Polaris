// <copyright file="ICaseInfoService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using System.Threading.Tasks;
using Common.Dto.Request;
using Common.Dto.Response.HouseKeeping;

/// <summary>
/// Interface for case info service that provides methods to retrieve and log case info.
/// </summary>
public interface ICaseInfoService
{
    /// <summary>
    /// Retrieves high-level display information for a specific case.
    /// </summary>
    /// <param name="caseId">The unique identifier of the case for which communications are being retrieved.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve communications.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only <see cref="CaseSummary"/> object.</returns>
    Task<CaseSummaryResponse> GetCaseInfoAsync(int caseId, CmsAuthValues cmsAuthValues);
}
