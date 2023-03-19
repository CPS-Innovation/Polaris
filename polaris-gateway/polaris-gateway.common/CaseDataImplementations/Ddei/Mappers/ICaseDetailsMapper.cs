using BusinessDomain = PolarisGateway.Domain.CaseData;
using Ddei.Domain;

namespace PolarisGateway.CaseDataImplementations.Ddei.Mappers
{
    public interface ICaseDetailsMapper
    {
        BusinessDomain.CaseDetailsFull MapCaseDetails(CaseDetails caseDetails);
    }
}