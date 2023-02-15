using BusinessDomain = PolarisAuthHandover.Domain.CaseData;
using ApiDomain = PolarisAuthHandover.CaseDataImplementations.Ddei.Domain;

namespace PolarisAuthHandover.CaseDataImplementations.Ddei.Mappers
{
    public interface ICaseDetailsMapper
    {
        BusinessDomain.CaseDetailsFull MapCaseDetails(ApiDomain.CaseDetails caseDetails);
    }
}