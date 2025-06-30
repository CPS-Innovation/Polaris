using Ddei.Domain.CaseData.Args.Core;

namespace Ddei.Factories;

public interface IDdeiAuthClientRequestFactory
{
    HttpRequestMessage CreateVerifyCmsAuthRequest(DdeiBaseArgDto arg);
}