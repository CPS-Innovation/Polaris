// <copyright file="MdsCaseDocumentsOrchestrationService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Services.MdsOrchestration;

using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Documents;
using Common.Dto.Response.HouseKeeping.Pcd;
using Common.Extensions;
using Common.Mappers;
using Common.Services.DocumentToggle;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Clients.Interfaces;
using PolarisGateway.Services.MdsOrchestration.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MdsCaseDocumentsOrchestrationService (
        IMdsClient mdsClient,
        IMasterDataServiceClient masterDataServiceClient,
        ICmsDocumentDtoMapper cmsDocumentDtoMapper,
        IMdsArgFactory mdsArgFactory,
        ICaseDetailsMapper caseDetailsMapper,
        IDocumentToggleService documentToggleService,
        IDocumentDtoMapper documentMapper)
    : IMdsCaseDocumentsOrchestrationService
{
    public async Task<IEnumerable<DocumentDto>> GetCaseDocuments(MdsCaseIdentifiersArgDto arg)
    {
        var getDocumentsTask = masterDataServiceClient.ListDocumentsAsync(arg, new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));
        var getPcdRequestsTask = masterDataServiceClient.GetCasePcdRequestsAsync(arg, new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));
        var getDefendantsAndChargesTask = masterDataServiceClient.GetCaseDefendantsAsync(
            new ListCaseDefendantsRequest(arg.CaseId, arg.CorrelationId),
            new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));

        await Task.WhenAll(getDocumentsTask, getPcdRequestsTask, getDefendantsAndChargesTask);

        var getDocumentsMapped = getDocumentsTask.Result.Select(x => cmsDocumentDtoMapper.Map(x, null)).ToList();

        var cmsDocuments = getDocumentsMapped;
        var pcdRequests = getPcdRequestsTask.Result;
        var defendantAndCharges = getDefendantsAndChargesTask.Result;
        var defendandAndChargesMapped = caseDetailsMapper.MapDefendantsResponseToDefendantsAndChargesListDto(defendantAndCharges, arg.CaseId);

        return Enumerable.Empty<DocumentDto>()
            .Concat(cmsDocuments.Select(this.MapDocument))
            .Concat(pcdRequests.Select(this.MapPcdRequest))
            .Concat(
                defendandAndChargesMapped.DefendantsAndCharges.Any() ||
                defendandAndChargesMapped.DefendantsAndCharges.Any(x => x.Charges.Any())
                    ? [this.MapDefendantAndCharges(defendandAndChargesMapped)]
                    : []
            );
    }

    public DocumentDto MapDocument(CmsDocumentDto document) =>
        documentMapper.Map(document, documentToggleService.GetDocumentPresentationFlags(document));

    public DocumentDto MapPcdRequest(Common.Dto.Response.HouseKeeping.Pcd.PcdRequestDto pcdRequest) =>
        documentMapper.Map(pcdRequest, documentToggleService.GetPcdRequestPresentationFlags(pcdRequest));

    public DocumentDto MapDefendantAndCharges(DefendantsAndChargesListDto defendantAndCharges) =>
        documentMapper.Map(defendantAndCharges, documentToggleService.GetDefendantAndChargesPresentationFlags(defendantAndCharges));
}
