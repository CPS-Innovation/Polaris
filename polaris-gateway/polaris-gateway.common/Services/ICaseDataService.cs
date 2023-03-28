using Common.Dto.Case;
using Ddei.Domain.CaseData.Args;

namespace PolarisGateway.Services
{
    public interface ICaseDataService
    {
        Task<IEnumerable<CaseDto>> ListCases(DdeiCmsUrnArgDto arg);

        Task<CaseDto> GetCase(DdeiCmsCaseArgDto arg);

        Task<IEnumerable<DocumentDetailsDto>> ListDocuments(DdeiCmsCaseArgDto arg);
    }
}