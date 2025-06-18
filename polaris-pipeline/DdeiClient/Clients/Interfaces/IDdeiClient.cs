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

public interface IDdeiClient
{
    Task VerifyCmsAuthAsync(DdeiBaseArgDto arg);

    Task<CaseIdentifiersDto> GetUrnFromCaseIdAsync(DdeiCaseIdOnlyArgDto arg);

    Task<IEnumerable<CaseDto>> ListCasesAsync(DdeiUrnArgDto arg);

    Task<CaseDto> GetCaseAsync(DdeiCaseIdentifiersArgDto arg);

    Task<CaseSummaryDto> GetCaseSummaryAsync(DdeiCaseIdOnlyArgDto arg);

    Task<IEnumerable<PcdRequestCoreDto>> GetPcdRequestsCoreAsync(DdeiCaseIdentifiersArgDto arg);

    Task<IEnumerable<PcdRequestDto>> GetPcdRequestsAsync(DdeiCaseIdentifiersArgDto arg);

    Task<PcdRequestDto> GetPcdRequestAsync(DdeiPcdArgDto arg);

    Task<DefendantsAndChargesListDto> GetDefendantAndChargesAsync(DdeiCaseIdentifiersArgDto arg);

    Task<IEnumerable<CmsDocumentDto>> ListDocumentsAsync(DdeiCaseIdentifiersArgDto arg);

    Task<FileResult> GetDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg);

    Task<Stream> GetDocumentFromFileStoreAsync(string path, string cmsAuthValues, Guid correlationId);

    Task<CheckoutDocumentDto> CheckoutDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg);

    Task CancelCheckoutDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg);

    Task<HttpResponseMessage> UploadPdfAsync(DdeiDocumentIdAndVersionIdArgDto arg, Stream stream);

    Task<IEnumerable<DocumentNoteDto>> GetDocumentNotesAsync(DdeiDocumentArgDto arg);

    Task<DocumentNoteResult> AddDocumentNoteAsync(DdeiAddDocumentNoteArgDto arg);

    Task<DocumentRenamedResultDto> RenameDocumentAsync(DdeiRenameDocumentArgDto arg);

    Task<DocumentReclassifiedResultDto> ReclassifyDocumentAsync(DdeiReclassifyDocumentArgDto arg);

    Task<DdeiCommunicationReclassifiedResponse> ReclassifyCommunicationAsync(DdeiReclassifyCommunicationArgDto arg);

    Task<IEnumerable<ExhibitProducerDto>> GetExhibitProducersAsync(DdeiCaseIdentifiersArgDto arg);

    Task<IEnumerable<BaseCaseWitnessResponse>> GetWitnessesAsync(DdeiCaseIdentifiersArgDto arg);

    Task<IEnumerable<MaterialTypeDto>> GetMaterialTypeListAsync(DdeiBaseArgDto arg);

    Task<IEnumerable<WitnessStatementDto>> GetWitnessStatementsAsync(DdeiWitnessStatementsArgDto arg);

    Task<bool> ToggleIsUnusedDocumentAsync(DdeiToggleIsUnusedDocumentDto toggleIsUnusedDocumentDto);

    Task<IEnumerable<DdeiCaseIdentifiersDto>> ListCaseIdsAsync(DdeiUrnArgDto arg);
}