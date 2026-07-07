using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using DdeiClient.Domain.Args;
using Ddei.Domain.Response.Document;
using Ddei.Domain.Response;
using System.Threading;

namespace DdeiClient.Clients.Interfaces;

public interface IMdsClient
{
    Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(MdsCaseIdOnlyArgDto arg, CancellationToken cancellationToken = default);

    Task<CaseSummaryDto> GetCaseSummaryAsync(MdsCaseIdOnlyArgDto arg, CancellationToken cancellationToken = default);

    Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequestsCoreAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default);

    Task<IEnumerable<PcdRequestDto>> GetPcdRequestsAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default);

    Task<PcdRequestDto> GetPcdRequestAsync(MdsPcdArgDto arg, CancellationToken cancellationToken = default);

    Task<DefendantsAndChargesListDto> GetDefendantAndChargesAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default);

    Task<IEnumerable<CmsDocumentDto>> ListDocumentsAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default);

    Task<FileResult> GetDocumentAsync(MdsMaterialIdAndDocumentIdArgDto arg, CancellationToken cancellationToken = default);

    Task<CheckoutDocumentDto> CheckoutDocumentAsync(MdsMaterialIdAndDocumentIdArgDto arg, CancellationToken cancellationToken = default);

    Task CancelCheckoutDocumentAsync(MdsMaterialIdAndDocumentIdArgDto arg, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> UploadPdfAsync(MdsMaterialIdAndDocumentIdArgDto arg, Stream stream, CancellationToken cancellationToken = default);

    Task<IEnumerable<DocumentNoteDto>> GetDocumentNotesAsync(MdsDocumentArgDto arg, CancellationToken cancellationToken = default);

    Task<DocumentNoteResult> AddDocumentNoteAsync(MdsAddDocumentNoteArgDto arg, CancellationToken cancellationToken = default);

    Task<DocumentRenamedResultDto> RenameDocumentAsync(MdsRenameDocumentArgDto arg, CancellationToken cancellationToken = default);

    Task<DocumentRenamedResultDto> RenameExhibitAsync(MdsRenameDocumentArgDto arg, CancellationToken cancellationToken = default);

    Task<MdsCommunicationReclassifiedResponse> ReclassifyCommunicationAsync(MdsReclassifyCommunicationArgDto arg, CancellationToken cancellationToken = default);

    Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducersAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default);

    Task<IEnumerable<BaseCaseWitnessResponse>> GetWitnessesAsync(MdsCaseIdentifiersArgDto arg, CancellationToken cancellationToken = default);

    Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(CmsBaseArgDto arg, CancellationToken cancellationToken = default);

    Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(MdsWitnessStatementsArgDto arg, CancellationToken cancellationToken = default);

    Task<bool> ToggleIsUnusedDocumentAsync(MdsToggleIsUnusedDocumentDto toggleIsUnusedDocumentDto, CancellationToken cancellationToken = default);

    Task<IEnumerable<MdsCaseIdentifiersDto>> ListCaseIdsAsync(MdsUrnArgDto arg, CancellationToken cancellationToken = default);
}