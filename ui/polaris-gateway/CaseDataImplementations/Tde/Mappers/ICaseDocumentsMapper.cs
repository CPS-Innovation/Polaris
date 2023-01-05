using BusinessDomain = PolarisGateway.Domain.CaseData;
using ApiDomain = PolarisGateway.CaseDataImplementations.Ddei.Domain;

namespace PolarisGateway.CaseDataImplementations.Ddei.Mappers
{
    public interface ICaseDocumentsMapper
    {
        BusinessDomain.DocumentDetails MapDocumentDetails(ApiDomain.DocumentDetails documentDetails);
    }
}