// <copyright file="ICaseLockService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using Common.Dto.Request;
using Common.Dto.Response.HouseKeeping;
using System.Threading.Tasks;

/// <summary>
/// Interface for case unlock service that provides a method to unlock the case.
/// </summary>
public interface ICaseLockService
{
    /// <summary>
    /// Asynchronously checks case lock status.
    /// </summary>
    /// <param name="caseId">The ID of the case to be unlocked.</param>
    /// <param name="cmsAuthValues">The CMS authentication values required for the API call.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a read-only string object.</returns>
    Task<CaseLockedStatusResult> CheckCaseLockAsync(int caseId, CmsAuthValues cmsAuthValues);
}
