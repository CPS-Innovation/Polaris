using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Documents;
using Common.Extensions;
using Common.Services.DocumentToggle;
using Ddei.Domain.CaseData.Args.Core;
using DdeiClient.Enums;
using DdeiClient.Factories;
using PolarisGateway.Services.DdeiOrchestration.Mappers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolarisGateway.Services.DdeiOrchestration;

public class DdeiCaseDocumentsOrchestrationService : IDdeiCaseDocumentsOrchestrationService
{
    private readonly IDdeiClientFactory _ddeiClientFactory;
    private readonly IDocumentToggleService _documentToggleService;
    private readonly IDocumentDtoMapper _cmsDocumentMapper;

    public DdeiCaseDocumentsOrchestrationService(
            IDdeiClientFactory ddeiClientFactory,
            IDocumentToggleService documentToggleService,
            IDocumentDtoMapper cmsDocumentMapper
        )
    {
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
        _documentToggleService = documentToggleService.ExceptionIfNull();
        _cmsDocumentMapper = cmsDocumentMapper.ExceptionIfNull();
    }

    public async Task<IEnumerable<DocumentDto>> GetCaseDocuments(DdeiCaseIdentifiersArgDto arg)
    {
        var mdsClient = _ddeiClientFactory.Create(arg.CmsAuthValues, DdeiClients.Mds);

        var getDocumentsTask = mdsClient.ListDocumentsAsync(arg);
        var getPcdRequestsTask = mdsClient.GetPcdRequestsCoreAsync(arg);
        var getDefendantsAndChargesTask = mdsClient.GetDefendantAndChargesAsync(arg);

        await Task.WhenAll(getDocumentsTask, getPcdRequestsTask, getDefendantsAndChargesTask);

        var cmsDocuments = getDocumentsTask.Result;
        var pcdRequests = getPcdRequestsTask.Result;
        var defendantAndCharges = getDefendantsAndChargesTask.Result;

        return Enumerable.Empty<DocumentDto>()
            .Concat(cmsDocuments.Select(MapDocument))
            .Concat(pcdRequests.Select(MapPcdRequest))
            .Concat(defendantAndCharges.DefendantsAndCharges.Count() > 1 || defendantAndCharges.DefendantsAndCharges.Any(x => x.Charges.Count() > 1)
                ? [MapDefendantAndCharges(defendantAndCharges)]
                : []
            );
    }

    public DocumentDto MapDocument(CmsDocumentDto document) =>
        _cmsDocumentMapper.Map(document, _documentToggleService.GetDocumentPresentationFlags(document));

    public DocumentDto MapPcdRequest(PcdRequestCoreDto pcdRequest) =>
        _cmsDocumentMapper.Map(pcdRequest, _documentToggleService.GetPcdRequestPresentationFlags(pcdRequest));

    public DocumentDto MapDefendantAndCharges(DefendantsAndChargesListDto defendantAndCharges) =>
        _cmsDocumentMapper.Map(defendantAndCharges, _documentToggleService.GetDefendantAndChargesPresentationFlags(defendantAndCharges));
}