using BusinessDomain = RumpoleGateway.Domain.CaseData;
using ApiDomain = RumpoleGateway.CaseDataImplementations.Tde.Domain;

namespace RumpoleGateway.CaseDataImplementations.Tde.Mappers
{
    public interface ICaseDetailsMapper
    {
        BusinessDomain.CaseDetailsFull MapCaseDetails(ApiDomain.CaseDetails caseDetails);
    }
}