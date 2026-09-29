// <copyright file="MdsCaseDocumentsOrchestrationService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Services.MdsOrchestration;

using Common.Dto.Request;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Documents;
using Common.Dto.Response.HouseKeeping.Pcd;
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
        //var getPcdRequestsTask = _mdsClient.GetPcdRequestsCoreAsync(arg); // calls /cases/{arg.CaseId}/pcd-requests/overview  returns PcdRequestCoreDto

        var getPcdRequestsTask = masterDataServiceClient.GetCasePcdRequestsAsync(arg, new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));

        var getDefendantsAndChargesTask = mdsClient.GetDefendantAndChargesAsync(arg);

        await Task.WhenAll(getDocumentsTask, getPcdRequestsTask, getDefendantsAndChargesTask);

        var cmsDocuments = getDocumentsTask.Result;
        var pcdRequests = getPcdRequestsTask.Result;
        var defendantAndCharges = getDefendantsAndChargesTask.Result;

        return Enumerable.Empty<DocumentDto>()
            .Concat(cmsDocuments.Select(this.MapDocument))
            .Concat(pcdRequests.Select(this.MapPcdRequest))
            .Concat(
                defendantAndCharges.DefendantsAndCharges.Any() ||
                defendantAndCharges.DefendantsAndCharges.Any(x => x.Charges.Any())
                    ? [this.MapDefendantAndCharges(defendantAndCharges)]
                    : []
            );
    }

    public DocumentDto MapDocument(CmsDocumentDto document) =>
        cmsDocumentMapper.Map(document, documentToggleService.GetDocumentPresentationFlags(document));

    public DocumentDto MapPcdRequest(Common.Dto.Response.HouseKeeping.Pcd.PcdRequestDto pcdRequest) =>
        cmsDocumentMapper.Map(pcdRequest, documentToggleService.GetPcdRequestPresentationFlags(pcdRequest));

    public DocumentDto MapDefendantAndCharges(DefendantsAndChargesListDto defendantAndCharges) =>
        cmsDocumentMapper.Map(defendantAndCharges, documentToggleService.GetDefendantAndChargesPresentationFlags(defendantAndCharges));
}
