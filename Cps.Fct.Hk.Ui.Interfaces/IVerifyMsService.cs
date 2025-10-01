// <copyright file="IVerifyMsService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// Interface for handling the Microsoft Entra ID token.
/// </summary>
public interface IVerifyMsService
{
    /// <summary>
    /// Processes the provided Microsoft Entra ID token and returns the result.
    /// </summary>
    /// <param name="idToken">The ID token to process. Can be <c>null</c> or empty.</param>
    /// <returns>
    /// A <see cref="Task{VerifyMsResult}"/> representing the asynchronous operation.
    /// The result contains details of the verification process.
    /// </returns>
    Task<VerifyMsResult> ProcessRequest(string? idToken);

    /// <summary>
    /// Extracts user specific claims from the provided Microsoft Entra Id token and log claims.
    /// </summary>
    /// <param name="idToken">The ID token to process.</param>
    /// <param name="caseId">The caseId associated with user.</param>
    void ExtractAndLogUserClaims(string idToken, int? caseId);
}
