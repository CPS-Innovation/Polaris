using Common.Dto.Request;

namespace PolarisGateway.Mappers
{
    public class ReclassifyDocumentRequestMapper : IReclassifyDocumentRequestMapper
    {
        public ReclassifyDocumentDto Map(ReclassifyDocumentDto documentReclassificationRequest)
        {
            return new ReclassifyDocumentDto
            {
                DocumentTypeId = documentReclassificationRequest.DocumentTypeId,
                Exhibit = documentReclassificationRequest.Exhibit,
                Statement = documentReclassificationRequest.Statement,
                Other = documentReclassificationRequest.Other,
                Immediate = documentReclassificationRequest.Immediate
            };
        }
    }
}