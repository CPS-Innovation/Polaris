using BusinessDomain = PolarisAuthHandover.Domain.CaseData;
using ApiDomain = PolarisAuthHandover.CaseDataImplementations.Ddei.Domain;

namespace PolarisAuthHandover.CaseDataImplementations.Ddei.Mappers
{
    public interface ICaseDocumentsMapper
    {
        BusinessDomain.DocumentDetails MapDocumentDetails(ApiDomain.DocumentDetails documentDetails);
    }
}