using Ddei.Domain.CaseData.Args;

namespace Ddei.Services.Contract
{
    public interface ICmsModernTokenService
    {
        Task<string> GetCmsModernToken(DdeiCmsCaseDataArgDto arg);

    }
}