using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response.Documents;
using Common.Services.DocumentToggle;
using Ddei;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using PolarisGateway.Services.DdeiOrchestration.Mappers;

namespace PolarisGateway.Services.DdeiOrchestration;

public class DdeiOrchestrationService : IDdeiOrchestrationService
{
    private readonly IDdeiClient _ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IDocumentToggleService _documentToggleService;
    private readonly IDocumentDtoMapper _cmsDocumentMapper;

    public DdeiOrchestrationService(
             IDdeiClient ddeiClient,
             IDdeiArgFactory ddeiArgFactory,
             IDocumentToggleService documentToggleService,
             IDocumentDtoMapper cmsDocumentMapper
        )
    {
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _documentToggleService = documentToggleService ?? throw new ArgumentNullException(nameof(documentToggleService));
        _cmsDocumentMapper = cmsDocumentMapper ?? throw new ArgumentNullException(nameof(cmsDocumentMapper));
    }

    public async Task<IEnumerable<DocumentDto>> GetCaseDocuments(DdeiCaseIdentifiersArgDto arg)
    {
        var getDocumentsTask = _ddeiClient.ListDocumentsAsync(arg);
        var getPcdRequestsTask = _ddeiClient.GetPcdRequestsAsync(arg);
        var getDefendantsAndChargesTask = _ddeiClient.GetDefendantAndChargesAsync(arg);

        await Task.WhenAll(getDocumentsTask, getPcdRequestsTask, getDefendantsAndChargesTask);

        var cmsDocuments = getDocumentsTask.Result;
        var pcdRequests = getPcdRequestsTask.Result;
        var defendantAndCharges = getDefendantsAndChargesTask.Result;

        return Enumerable.Empty<DocumentDto>()
            .Concat(cmsDocuments.Select(doc => MapDocument(doc)))
            .Concat(pcdRequests.Select(pcd => MapPcdRequest(pcd)))
            .Concat(defendantAndCharges.DefendantsAndCharges.Count() > 1
                ? new[] { MapDefendantAndCharges(defendantAndCharges) }
                : Enumerable.Empty<DocumentDto>()
            );
    }

    public DocumentDto MapDocument(CmsDocumentDto document) =>
        _cmsDocumentMapper.Map(document, _documentToggleService.GetDocumentPresentationFlags(document));

    public DocumentDto MapPcdRequest(PcdRequestCoreDto pcdRequest) =>
        _cmsDocumentMapper.Map(pcdRequest, _documentToggleService.GetPcdRequestPresentationFlags(pcdRequest));

    public DocumentDto MapDefendantAndCharges(DefendantsAndChargesListDto defendantAndCharges) =>
        _cmsDocumentMapper.Map(defendantAndCharges, _documentToggleService.GetDefendantAndChargesPresentationFlags(defendantAndCharges));
}