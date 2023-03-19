using Ddei.Domain;
using BusinessDomain = PolarisGateway.Domain.CaseData;

namespace PolarisGateway.CaseDataImplementations.Ddei.Mappers
{
    public interface ICaseDocumentsMapper
    {
        BusinessDomain.DocumentDetails MapDocumentDetails(DocumentDetails documentDetails);
    }
}