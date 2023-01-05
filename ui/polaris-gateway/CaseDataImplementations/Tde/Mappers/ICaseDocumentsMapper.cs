using BusinessDomain = RumpoleGateway.Domain.CaseData;
using ApiDomain = RumpoleGateway.CaseDataImplementations.Tde.Domain;

namespace RumpoleGateway.CaseDataImplementations.Tde.Mappers
{
    public interface ICaseDocumentsMapper
    {
        BusinessDomain.DocumentDetails MapDocumentDetails(ApiDomain.DocumentDetails documentDetails);
    }
}