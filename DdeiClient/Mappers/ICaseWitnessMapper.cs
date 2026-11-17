// <copyright file="ICaseWitnessMapper.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Ddei.Mappers;

using Common.Dto.Response.Case;
using Common.Dto.Response;
using DdeiClient.Domain.Response;
using Common.Dto.Response.HouseKeeping;

public interface ICaseWitnessMapper
{
    CaseWitnessDto Map(Witness witness);
}
