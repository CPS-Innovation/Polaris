// <copyright file="IDocumentDtoMapper.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Services.MdsOrchestration.Mappers;

using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;
using Common.Dto.Response.Documents;

public interface IDocumentDtoMapper
{
    DocumentDto Map(CmsDocumentDto document, PresentationFlagsDto presentationFlagsDto);

    DocumentDto Map(Common.Dto.Response.HouseKeeping.Pcd.PcdRequestDto pcdRequest, PresentationFlagsDto presentationFlagsDto);

    DocumentDto Map(DefendantsAndChargesListDto defendantAndCharges, PresentationFlagsDto presentationFlagsDto);
}
