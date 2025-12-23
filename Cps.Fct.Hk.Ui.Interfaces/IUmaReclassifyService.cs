// <copyright file="IUmaReclassifyService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Interfaces;

using System.Threading.Tasks;
using Common.Dto.Response.HouseKeeping;

/// <summary>
/// Interface for reclassifying using unused materials automation.
/// </summary>
public interface IUmaReclassifyService
{
    /// <summary>
    /// Processes the reclassification request for a given collection of communications.
    /// </summary>
    /// <param name="caseId">the case Id associated with the communications.</param>
    /// <param name="communications">The collection of communications to be matched.</param>
    /// <returns>
    /// A task representing the asynchronous operation,
    /// containing a read-only collection of matched communications.
    /// </returns>
    Task<IReadOnlyCollection<MatchedCommunication>> ProcessMatchingRequest(int caseId, IReadOnlyCollection<Communication> communications);
}
