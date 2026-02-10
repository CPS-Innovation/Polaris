// <copyright file="IMaterialReclassificationOrchestrationService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>
namespace Cps.Fct.Hk.Ui.Interfaces;

using System.Threading.Tasks;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;

/// <summary>
/// Provides a service for complte reclassification of a case material.
/// </summary>
public interface IMaterialReclassificationOrchestrationService
{
    /// <summary>
    ///  Provides a service for complte reclassification of a case material.
    /// </summary>
    /// <param name="caseId">The unique identifier of the case.</param>
    /// <param name="materialId">The material id belonging to the case material to reclassify.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <param name="request">The complete reclassification request.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<CompleteReclassificationResponse> CompleteReclassificationAsync(
        int caseId,
        int materialId,
        CmsAuthValues cmsAuthValues,
        CompleteReclassificationRequest request);
}
