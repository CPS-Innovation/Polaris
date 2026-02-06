using Common.Dto.Response.Document;
using Ddei.Domain.CaseData.Args;
using Ddei.Domain.CaseData.Args.Core;

namespace DdeiClient.Clients.Interfaces;

public interface IDdeiAuthClient
{
    Task VerifyCmsAuthAsync(CmsBaseArgDto arg);
}