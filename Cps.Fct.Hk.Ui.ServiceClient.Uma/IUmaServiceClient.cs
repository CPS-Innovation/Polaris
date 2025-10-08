// <copyright file="IUmaServiceClient.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.ServiceClient.Uma;

using System.Threading.Tasks;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces.Model;

/// <summary>
/// The interface for the UMA API service client.
/// </summary>
public interface IUmaServiceClient
{
    /// <summary>
    /// Fetches the matched communications for a case identifier.
    /// </summary>
    /// <param name="caseId">the caseId associated with the communications.</param>
    /// <param name="communications">The collection of communications to be matched.</param>
    /// <returns>A collection of matched communications with materialId and subject.</returns>
    Task<IReadOnlyCollection<MatchedCommunication>> MatchCommunicationsUmAsync(int caseId, IReadOnlyCollection<Communication> communications);
}
