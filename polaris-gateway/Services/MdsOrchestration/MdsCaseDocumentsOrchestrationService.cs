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

namespace PolarisGateway.Services.MdsOrchestration;

public class MdsCaseDocumentsOrchestrationService : IMdsCaseDocumentsOrchestrationService
{
    private readonly IMdsClient _mdsClient;
    private readonly IMasterDataServiceClient _masterDataServiceClient;
    private readonly IDocumentToggleService _documentToggleService;
    private readonly IDocumentDtoMapper _cmsDocumentMapper;

    public MdsCaseDocumentsOrchestrationService(
            IMdsClient mdsClient,
            IMasterDataServiceClient masterDataServiceClient,
            IMdsArgFactory mdsArgFactory,
            IDocumentToggleService documentToggleService,
            IDocumentDtoMapper cmsDocumentMapper
        )
    {
        _mdsClient = mdsClient.ExceptionIfNull();
        _masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
        _documentToggleService = documentToggleService.ExceptionIfNull();
        _cmsDocumentMapper = cmsDocumentMapper.ExceptionIfNull();
    }

    public async Task<IEnumerable<DocumentDto>> GetCaseDocuments(MdsCaseIdentifiersArgDto arg)
    {
        var getDocumentsTask = _mdsClient.ListDocumentsAsync(arg);
        //var getPcdRequestsTask = _mdsClient.GetPcdRequestsCoreAsync(arg);
        var getPcdRequestsTask = _masterDataServiceClient.GetPcdRequestCoreAsync(
            new GetPcdRequestsCoreRequest(arg.CaseId, arg.CorrelationId),
            new CmsAuthValues(arg.CmsAuthValues, arg.CorrelationId));
        var getDefendantsAndChargesTask = _mdsClient.GetDefendantAndChargesAsync(arg);

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
        _cmsDocumentMapper.Map(document, _documentToggleService.GetDocumentPresentationFlags(document));

    public DocumentDto MapPcdRequest(PcdRequestCoreDto pcdRequest) =>
        _cmsDocumentMapper.Map(pcdRequest, _documentToggleService.GetPcdRequestPresentationFlags(pcdRequest));

    public DocumentDto MapDefendantAndCharges(DefendantsAndChargesListDto defendantAndCharges) =>
        _cmsDocumentMapper.Map(defendantAndCharges, _documentToggleService.GetDefendantAndChargesPresentationFlags(defendantAndCharges));
}