using Ddei.Domain.CaseData.Args;
using PolarisGateway.Domain.CaseData;

namespace PolarisGateway.Services
{
    public interface ICaseDataService
    {
        Task<IEnumerable<CaseDetails>> ListCases(DdeiCmsUrnArgDto arg);

        Task<CaseDetailsFull> GetCase(DdeiCmsCaseArgDto arg);

        Task<IEnumerable<DocumentDetails>> ListDocuments(DdeiCmsCaseArgDto arg);
    }
}