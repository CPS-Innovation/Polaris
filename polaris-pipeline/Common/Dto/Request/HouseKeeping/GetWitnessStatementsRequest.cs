// <copyright file="GetStatementsForWitnessRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;
using System;

/// <summary>
/// The request to get the statements for a witness.
/// </summary>
/// <param name="CaseId">The witness ID.</param>
/// <param name="CorrespondenceId">The correspondence ID.</param>
public record GetWitnessStatementsRequest(int WitnessId, Guid CorrespondenceId)
     : BaseRequest(CorrespondenceId: CorrespondenceId)
{

}
