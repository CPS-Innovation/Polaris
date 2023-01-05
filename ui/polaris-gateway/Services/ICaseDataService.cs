using System.Collections.Generic;
using System.Threading.Tasks;
using RumpoleGateway.Domain.CaseData;
using RumpoleGateway.Domain.CaseData.Args;

namespace RumpoleGateway.Services
{
    public interface ICaseDataService
    {
        Task<IEnumerable<CaseDetails>> ListCases(UrnArg arg);

        Task<CaseDetailsFull> GetCase(CaseArg arg);

        Task<IEnumerable<DocumentDetails>> ListDocuments(CaseArg arg);
    }
}