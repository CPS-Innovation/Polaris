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
    /// <param name="caseId">The unique identifier of the case.</param>
    /// <param name="materialId">The unique identifier of the material to reclassify.</param>
    /// <param name="classification">The type of classification to apply.</param>
    /// <param name="documentTypeId">The document type id.</param>
    /// <param name="used">Flag to indicate if material is used or unsed.</param>
    /// <param name="subject">The material subject.</param>
    /// <param name="cmsAuthValues">Authorization values for CMS access.</param>
    /// <param name="statement">The statement request.</param>
    /// <param name="exhibit">Exhibit request.</param>
    /// <param name="correspondenceId">The correspndence identifier.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<ReclassificationResponse> ReclassifyCaseMaterialAsync(int caseId, int materialId, string classification, int documentTypeId, bool used, string subject, CmsAuthValues cmsAuthValues, ReclassifyStatementRequest? statement = null, ReclassifyExhibitRequest? exhibit = null, Guid correspondenceId = default);
}
