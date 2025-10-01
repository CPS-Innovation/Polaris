// <copyright file="IWitnessService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// Interfaces for witness related sevices.
/// </summary>
public interface IWitnessService
{
    /// <summary>
    /// Service that retrieves witnesses for a case.
    /// </summary>
    /// <param name="caseId">Unique identifier of the case to retrieve witnesses for.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve witnesses.</param>
    /// <returns>A task <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<WitnessesResponse> GetWitnessesForCaseAsync(int caseId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Service that retrieves statements for a witness.
    /// </summary>
    /// <param name="witnessId">Unique witness identifier to retrieve statements for.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve statements for a witness.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<StatementsForWitnessResponse> GetStatementsForWitnessAsync(int witnessId, CmsAuthValues cmsAuthValues);

    /// <summary>
    /// Service that adds a case witness.
    /// </summary>
    /// <param name="urn">The urn of the case for which a witness is being added.</param>
    /// <param name="caseId">The case ID to add witness to.</param>
    /// <param name="firstName">Witness first name.</param>
    /// <param name="lastName">Witness last name.</param>
    /// <param name="cmsAuthValues">The authentication values used to authorize the request to retrieve statements for a witness.</param>
    /// <param name="correspondenceId">correspondenceId.</param>
    /// <returns>The Id of the witness added.</returns>
    Task<int?> AddWitnessAsync(string urn, int caseId, string firstName, string lastName, CmsAuthValues cmsAuthValues, Guid correspondenceId = default);
}
