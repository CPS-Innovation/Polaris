using System.Threading.Tasks;
using PolarisAuthHandover.Domain.CaseData.Args;

namespace PolarisAuthHandover.Services
{
    public interface ICmsModernTokenService
    {
        Task<string> GetCmsModernToken(CaseDataArg arg);

    }
}