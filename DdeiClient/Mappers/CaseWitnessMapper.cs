// <copyright file="CaseWitnessMapper.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Ddei.Mappers;

using Common.Dto.Response.Case;
using Common.Dto.Response;
using DdeiClient.Domain.Response;
using Common.Dto.Response.HouseKeeping;

public class CaseWitnessMapper : ICaseWitnessMapper
{
    public CaseWitnessDto Map(Witness witness)
    {
        return new CaseWitnessDto
        {
            Id = (int)witness.WitnessId,
            Name = $"{witness.FirstName} {witness.Surname}",
        };
    }
}
