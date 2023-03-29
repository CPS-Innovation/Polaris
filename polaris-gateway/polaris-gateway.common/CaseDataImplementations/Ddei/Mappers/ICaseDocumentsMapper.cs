using Common.Dto.Case;
using Ddei.Domain;

namespace PolarisGateway.CaseDataImplementations.Ddei.Mappers
{
    public interface ICaseDocumentsMapper
    {
        DocumentDetailsDto MapDocumentDetails(DdeiDocumentDetailsDto documentDetails);
    }
}