using System.Threading.Tasks;
using PolarisGateway.Domain.CaseData.Args;

namespace PolarisGateway.Services
{
    public interface ICmsModernTokenService
    {
        Task<string> GetCmsModernToken(CaseDataArg arg);

    }
}