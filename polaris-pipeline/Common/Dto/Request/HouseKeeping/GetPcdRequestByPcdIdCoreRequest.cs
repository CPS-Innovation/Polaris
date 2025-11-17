// <copyright file="GetPcdRequestsCoreRequest.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>
namespace Common.Dto.Request.HouseKeeping;
using System;

/// <summary>
/// Get PCD request overview by PCD id and case id .
/// </summary>
/// <param name="caseId"></param>
///  <param name="pcdId"></param>
/// <param name="CorrespondenceId"></param>
public record GetPcdRequestByPcdIdCoreRequest(int caseId, int pcdId, Guid CorrespondenceId)
        : BaseRequest(CorrespondenceId: CorrespondenceId)
{
}
