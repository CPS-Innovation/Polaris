using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Common.Dto.Response;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;
using DdeiClient.Domain.Args;
using Ddei.Domain.Response.Document;
using Ddei.Domain.Response;

namespace DdeiClient.Clients.Interfaces;

public interface IMdsClient
{
    Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(MdsCaseIdOnlyArgDto arg);

    Task<CaseSummaryDto> GetCaseSummaryAsync(MdsCaseIdOnlyArgDto arg, System.Threading.CancellationToken cancellationToken = default);

    Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequestsCoreAsync(MdsCaseIdentifiersArgDto arg);

    Task<IEnumerable<PcdRequestDto>> GetPcdRequestsAsync(MdsCaseIdentifiersArgDto arg, System.Threading.CancellationToken cancellationToken = default);

    Task<PcdRequestDto> GetPcdRequestAsync(MdsPcdArgDto arg, System.Threading.CancellationToken cancellationToken = default);

    Task<DefendantsAndChargesListDto> GetDefendantAndChargesAsync(MdsCaseIdentifiersArgDto arg, System.Threading.CancellationToken cancellationToken = default);

    Task<IEnumerable<CmsDocumentDto>> ListDocumentsAsync(MdsCaseIdentifiersArgDto arg);

    Task<FileResult> GetDocumentAsync(MdsDocumentIdAndVersionIdArgDto arg, System.Threading.CancellationToken cancellationToken = default);

    Task<CheckoutDocumentDto> CheckoutDocumentAsync(MdsDocumentIdAndVersionIdArgDto arg);

    Task CancelCheckoutDocumentAsync(MdsDocumentIdAndVersionIdArgDto arg);

    Task<HttpResponseMessage> UploadPdfAsync(MdsDocumentIdAndVersionIdArgDto arg, Stream stream);

    Task<IEnumerable<DocumentNoteDto>> GetDocumentNotesAsync(MdsDocumentArgDto arg);

    Task<DocumentNoteResult> AddDocumentNoteAsync(MdsAddDocumentNoteArgDto arg);

    Task<DocumentRenamedResultDto> RenameDocumentAsync(MdsRenameDocumentArgDto arg);

    Task<DocumentRenamedResultDto> RenameExhibitAsync(MdsRenameDocumentArgDto arg);

    Task<MdsCommunicationReclassifiedResponse> ReclassifyCommunicationAsync(MdsReclassifyCommunicationArgDto arg);

    Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducersAsync(MdsCaseIdentifiersArgDto arg);

    Task<IEnumerable<BaseCaseWitnessResponse>> GetWitnessesAsync(MdsCaseIdentifiersArgDto arg, System.Threading.CancellationToken cancellationToken = default);

    Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(CmsBaseArgDto arg);

    Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(MdsWitnessStatementsArgDto arg);

    Task<bool> ToggleIsUnusedDocumentAsync(MdsToggleIsUnusedDocumentDto toggleIsUnusedDocumentDto);

    Task<IEnumerable<MdsCaseIdentifiersDto>> ListCaseIdsAsync(MdsUrnArgDto arg, System.Threading.CancellationToken cancellationToken = default);
}