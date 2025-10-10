using Common.Dto.Response.Document;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace DdeiClient.Clients.Interfaces;

public interface IDdeiAuthClient
{
    Task VerifyCmsAuthAsync(DdeiBaseArgDto arg);
    Task<DocumentReclassifiedResultDto> ReclassifyDocumentAsync(DdeiReclassifyDocumentArgDto arg);
    Task<DocumentRenamedResultDto> RenameDocumentAsync(DdeiRenameDocumentArgDto arg);
}