using Common.Dto.Case;
using Ddei.Domain;

namespace PolarisGateway.CaseDataImplementations.Ddei.Mappers
{
    public interface ICaseDetailsMapper
    {
        CaseDto MapCaseDetails(DdeiCaseDetailsDto caseDetails);
    }
}