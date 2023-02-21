using System.Collections.Generic;
using System.Threading.Tasks;
using PolarisAuthHandover.Domain.CaseData;
using PolarisAuthHandover.Domain.CaseData.Args;

namespace PolarisAuthHandover.Services
{
    public interface ICaseDataService
    {
        Task<IEnumerable<CaseDetails>> ListCases(UrnArg arg);

        Task<CaseDetailsFull> GetCase(CaseArg arg);

        Task<IEnumerable<DocumentDetails>> ListDocuments(CaseArg arg);
    }
}