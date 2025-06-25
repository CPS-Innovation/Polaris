using Ddei.Domain.CaseData.Args.Core;

namespace DdeiClient.Clients.Interfaces;

public interface IDdeiAuthClient
{
    Task VerifyCmsAuthAsync(DdeiBaseArgDto arg);
}