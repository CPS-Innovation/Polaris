using System.Collections.Generic;
using System.Threading.Tasks;
using RumpoleGateway.Domain.CaseData;
using RumpoleGateway.Domain.CaseData.Args;
using RumpoleGateway.Services;

namespace RumpoleGateway.CaseDataImplementations.Cda.Services
{
    public class CdaCaseDataService : ICaseDataService
    {
        public Task<CaseDetailsFull> GetCase(CaseArg arg)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<CaseDetails>> ListCases(UrnArg arg)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<DocumentDetails>> ListDocuments(CaseArg arg)
        {
            throw new System.NotImplementedException();
        }
    }
}