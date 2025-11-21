using Common.Dto.Response;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace DdeiClient.Clients.Interfaces;

public interface IDdeiAuthClient
{
    Task VerifyCmsAuthAsync(DdeiBaseArgDto arg);

    Task<FileResult> GetDocumentAsync(DdeiDocumentIdAndVersionIdArgDto arg);
}