using Ddei.Domain.CaseData.Args;
using PolarisGateway.Domain.CaseData;

namespace PolarisGateway.Services
{
    public interface ICaseDataService
    {
        Task<IEnumerable<CaseDetails>> ListCases(CmsUrnArg arg);

        Task<CaseDetailsFull> GetCase(CmsCaseArg arg);

        Task<IEnumerable<DocumentDetails>> ListDocuments(CmsCaseArg arg);
    }
}