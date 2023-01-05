using System.Collections.Generic;
using System.Threading.Tasks;
using PolarisGateway.Domain.CaseData;
using PolarisGateway.Domain.CaseData.Args;
using PolarisGateway.Services;

namespace PolarisGateway.CaseDataImplementations.Cda.Services
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