using BusinessDomain = PolarisGateway.Domain.CaseData;
using ApiDomain = PolarisGateway.CaseDataImplementations.Ddei.Domain;

namespace PolarisGateway.CaseDataImplementations.Ddei.Mappers
{
    public interface ICaseDetailsMapper
    {
        BusinessDomain.CaseDetailsFull MapCaseDetails(ApiDomain.CaseDetails caseDetails);
    }
}