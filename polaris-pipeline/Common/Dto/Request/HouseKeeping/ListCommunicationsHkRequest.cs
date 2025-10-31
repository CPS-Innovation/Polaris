// <copyright file="ListCommunicationsHkRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Common.Dto.Request.HouseKeeping;

using System;

/// <summary>
/// The request to list communications.
/// </summary>
/// <param name="CaseId">The numeric case ID.</param>
/// <param name="CorrespondenceId">The correspondence ID.</param>
public record ListCommunicationsHkRequest(int CaseId, Guid CorrespondenceId)
        : BaseRequest(CorrespondenceId: CorrespondenceId)
{
}

