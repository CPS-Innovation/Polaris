using Common.Dto.Case;
using Ddei.Domain;

namespace DdeiClient.Mappers
{
    public interface ICaseDetailsMapper
    {
        CaseDto MapCaseDetails(DdeiCaseDetailsDto caseDetails);
    }
}