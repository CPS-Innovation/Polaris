// <copyright file="MdsCaseDocumentsOrchestrationService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Services.MdsOrchestration;

using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Documents;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Request;
using Common.Extensions;
using Common.Services.DocumentToggle;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using PolarisGateway.Services.MdsOrchestration.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MdsCaseDocumentsOrchestrationService (
        IMdsClient mdsClient,
        IMasterDataServiceClient masterDataServiceClient,
        IMdsArgFactory mdsArgFactory,
        IDocumentToggleService documentToggleService,
        IDocumentDtoMapper cmsDocumentMapper)
    : IMdsCaseDocumentsOrchestrationService
{
    public async Task<IEnumerable<DocumentDto>> GetCaseDocuments(MdsCaseIdentifiersArgDto arg)
    {
        var getDocumentsTask = mdsClient.ListDocumentsAsync(arg);
        //var getPcdRequestsTask1 = mdsClient.GetPcdRequestsCoreAsync(arg); // returns resp.case,prech PcdRequestCoreDto
        var getPcdRequestsTask = masterDataServiceClient.GetPcdRequestCoreAsync(
            new GetPcdRequestsCoreRequest(arg.CaseId, arg.CorrelationId),
            new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));
        var getDefendantsAndChargesTask = mdsClient.GetDefendantAndChargesAsync(arg);

        await Task.WhenAll(getDocumentsTask, getPcdRequestsTask, getDefendantsAndChargesTask);

        var cmsDocuments = getDocumentsTask.Result;
        var pcdRequests = getPcdRequestsTask.Result;
        var defendantAndCharges = getDefendantsAndChargesTask.Result;

        // tahmeedChange
        return Enumerable.Empty<DocumentDto>()
            .Concat(cmsDocuments.Select(MapDocument))
            .Concat(
                pcdRequests
                    .Select(x => new PcdRequestCoreDto
                    {
                        Id = x.Id,
                        DecisionRequiredBy = x.DecisionRequiredBy,
                        DecisionRequested = x.DecisionRequested,
                    })
                    .Select(MapPcdRequest))
            .Concat(
                defendantAndCharges.DefendantsAndCharges.Any() ||
                defendantAndCharges.DefendantsAndCharges.Any(x => x.Charges.Any())
                    ? [MapDefendantAndCharges(defendantAndCharges)]
                    : []
            );

        //return Enumerable.Empty<DocumentDto>()
        //    .Concat(cmsDocuments.Select(MapDocument))
        //    .Concat(pcdRequests.Select(MapPcdRequest))
        //    .Concat(
        //        defendantAndCharges.DefendantsAndCharges.Any() ||
        //        defendantAndCharges.DefendantsAndCharges.Any(x => x.Charges.Any())
        //            ? [MapDefendantAndCharges(defendantAndCharges)]
        //            : []
        //    );
    }

    public DocumentDto MapDocument(CmsDocumentDto document) =>
        cmsDocumentMapper.Map(document, documentToggleService.GetDocumentPresentationFlags(document));

    public DocumentDto MapPcdRequest(PcdRequestCoreDto pcdRequest) =>
        cmsDocumentMapper.Map(pcdRequest, documentToggleService.GetPcdRequestPresentationFlags(pcdRequest));

    public DocumentDto MapDefendantAndCharges(DefendantsAndChargesListDto defendantAndCharges) =>
        cmsDocumentMapper.Map(defendantAndCharges, documentToggleService.GetDefendantAndChargesPresentationFlags(defendantAndCharges));
}
