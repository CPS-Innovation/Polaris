using System.Collections.Generic;
using System.Threading.Tasks;
using PolarisGateway.Domain.CaseData;
using PolarisGateway.Domain.CaseData.Args;

namespace PolarisGateway.Services
{
    public interface ICaseDataService
    {
        Task<IEnumerable<CaseDetails>> ListCases(UrnArg arg);

        Task<CaseDetailsFull> GetCase(CaseArg arg);

        Task<IEnumerable<DocumentDetails>> ListDocuments(CaseArg arg);
    }
}