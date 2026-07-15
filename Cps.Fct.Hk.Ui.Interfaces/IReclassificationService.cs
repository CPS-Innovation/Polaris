// <copyright file="IReclassificationService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using System;
using System.Threading.Tasks;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;

/// <summary>
/// Provides a service for reclassifying case materials.
/// </summary>
public interface IReclassificationService
{
    /// <summary>
    /// Reclassify material to classification type provided.
    /// </summary>
    /// <param name="request">The reclassification request containing all material and classification details.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <param name="cancellationToken">The cancellation token used to cancel the operation.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<ReclassificationResponse> ReclassifyCaseMaterialAsync(
        ReclassifyCaseMaterialServiceRequest request,
        CmsAuthValues cmsAuthValues,
        CancellationToken cancellationToken = default);
}
