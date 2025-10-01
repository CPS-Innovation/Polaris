// <copyright file="IInitService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using Cps.Fct.Hk.Ui.Interfaces.Model;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Defines the contract for a service that processes initialization requests.
/// </summary>
public interface IInitService
{
    /// <summary>
    /// Processes the initialization request and returns the result.
    /// </summary>
    /// <param name="req">The HTTP request that contains the necessary context for the initialization process.</param>
    /// <param name="caseId">The case ID to be processed. It is a required parameter.</param>
    /// <param name="cc">An optional parameter representing the CMS session cookie.</param>
    /// <param name="displayContext">An optional parameter representing the display context for the Housekeeping UI.</param>
    /// <returns>An <see cref="InitResult"/> containing the result of the initialization process.</returns>
    Task<InitResult> ProcessRequest(HttpRequest req, int? caseId, string? cc, string? displayContext);
}
