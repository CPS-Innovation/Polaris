// <copyright file="IBulkSetUnusedService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using Cps.Fct.Hk.Ui.Interfaces.Model;
using System.Threading.Tasks;

/// <summary>
/// Interface for bulk set service that provides methods to set materials to unused in bulk.
/// </summary>
public interface IBulkSetUnusedService
{
    /// <summary>
    /// Bulk sets materials as unused for a specified case.
    /// </summary>
    /// <param name="caseId">The case ID.</param>
    /// <param name="cmsAuthValues">The authentication values for CMS access.</param>
    /// <param name="bulkSetUnusedRequests">The materials to reclassify as unused.</param>
    /// <returns>A task that returns the bulk set response.</returns>
    Task<BulkSetUnusedResponse> BulkSetUnusedAsync(int caseId, CmsAuthValues cmsAuthValues, IReadOnlyCollection<BulkSetUnusedRequest> bulkSetUnusedRequests);
}
