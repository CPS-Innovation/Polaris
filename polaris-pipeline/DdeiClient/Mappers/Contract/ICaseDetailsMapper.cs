using Common.Dto.Case;
using Ddei.Domain;

namespace DdeiClient.Mappers.Contract
{
    public interface ICaseDetailsMapper
    {
        CaseDto MapCaseDetails(DdeiCaseDetailsDto caseDetails);
    }
}