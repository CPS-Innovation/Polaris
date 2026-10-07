// <copyright file="GetCaseDocuments.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Durable.Activity;

using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.HouseKeeping.Pcd;
using Common.Extensions;
using Common.Mappers;
using Common.Services.DocumentToggle;
using coordinator.Domain;
using coordinator.Durable.Payloads;
using coordinator.Services;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Clients.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

public class GetCaseDocuments(
    IMasterDataServiceClient masterDataServiceClient,
    IMdsArgFactory mdsArgFactory,
    ICaseDetailsMapper caseDetailsMapper,
    ICmsDocumentDtoMapper cmsDocumentDtoMapper,
    IDocumentToggleService documentToggleService,
    IStateStorageService stateStorageService)
{
    [Function(nameof(GetCaseDocuments))]
    public async Task<GetCaseDocumentsResponse> Run([ActivityTrigger] CasePayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.Urn))
        {
            throw new ArgumentException("CaseUrn cannot be empty");
        }

        if (payload.CaseId == 0)
        {
            throw new ArgumentException("CaseId cannot be zero");
        }

        if (string.IsNullOrWhiteSpace(payload.CmsAuthValues))
        {
            throw new ArgumentException("Cms Auth Token cannot be null");
        }

        if (payload.CorrelationId == Guid.Empty)
        {
            throw new ArgumentException("CorrelationId must be valid GUID");
        }

        var arg = mdsArgFactory.CreateCaseIdentifiersArg(
            payload.CmsAuthValues,
            payload.CorrelationId,
            payload.Urn,
            payload.CaseId);

        var getDocumentsTask = masterDataServiceClient.ListDocumentsAsync(arg, new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));
        var getPcdRequestsTask = masterDataServiceClient.GetCasePcdRequestsAsync(
            arg,
            new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));
        var getDefendantsAndChargesTask = masterDataServiceClient.GetCaseDefendantsAsync(
            new ListCaseDefendantsRequest(arg.CaseId, arg.CorrelationId),
            new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));

        await Task.WhenAll(getDocumentsTask, getPcdRequestsTask, getDefendantsAndChargesTask);

        var getDocumentsTaskMapped = getDocumentsTask.Result.Select(x => cmsDocumentDtoMapper.Map(x, null)).ToList();

        var cmsDocuments = getDocumentsTaskMapped
            .Select(this.MapPresentationFlags)
            .ToArray();

        var pcdRequests = getPcdRequestsTask.Result
            .Select(this.MapPresentationFlags)
            .ToArray();

        var defendantsAndCharges = getDefendantsAndChargesTask.Result;
        var defendandAndChargesMapped = caseDetailsMapper.MapDefendantsResponseToDefendantsAndChargesListDto(defendantsAndCharges, arg.CaseId);
        this.MapPresentationFlags(defendandAndChargesMapped);

        var documents = new GetCaseDocumentsResponse(cmsDocuments, pcdRequests, defendandAndChargesMapped);
        await stateStorageService.UpdateCaseDocumentsAsync(payload.CaseId, documents);

        return documents;
    }

    private CmsDocumentDto MapPresentationFlags(CmsDocumentDto document)
    {
        document.PresentationFlags = documentToggleService.GetDocumentPresentationFlags(document);
        return document;
    }

    // need to refactor this if we add PresentationFlags to MDS response and HK PcdRequestDto
    private PcdRequestCoreDto MapPresentationFlags(Common.Dto.Response.HouseKeeping.Pcd.PcdRequestDto pcdRequest)
    {
        PcdRequestCoreDto pcdRequestCoreDto = new PcdRequestCoreDto
        {
            Id = pcdRequest.Id,
            DecisionRequiredBy = pcdRequest.DecisionRequiredBy,
            DecisionRequested = pcdRequest.DecisionRequested,
        };
        pcdRequestCoreDto.PresentationFlags = documentToggleService.GetPcdRequestPresentationFlags(pcdRequest);
        return pcdRequestCoreDto;
    }

    private DefendantsAndChargesListDto MapPresentationFlags(DefendantsAndChargesListDto defendantsAndCharges)
    {
        if (defendantsAndCharges == null)
        {
            return null;
        }

        defendantsAndCharges.PresentationFlags = documentToggleService.GetDefendantAndChargesPresentationFlags(defendantsAndCharges);
        return defendantsAndCharges;
    }
}
