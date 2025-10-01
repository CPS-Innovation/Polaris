// <copyright file="IDocumentService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using Cps.Fct.Hk.Ui.Interfaces.Model;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

/// <summary>
/// Interface for document service that provides methods to retrieve and preview documents.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Downloads a material.
    /// </summary>
    /// <param name="caseId">The ID of the case.</param>
    /// <param name="link">The file path of the case material document to be downloaded.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve the unused materials.</param>
    /// <param name="firstPageOnly">A flag to indicate if the request is first page only or full document.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="FileStreamResult"/> object, the downloaded material.</returns>
    Task<FileStreamResult?> GetMaterialDocumentAsync(string caseId, string link, CmsAuthValues cmsAuthValues, bool firstPageOnly = true);
}
