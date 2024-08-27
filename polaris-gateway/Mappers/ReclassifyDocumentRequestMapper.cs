using Common.Dto.Request;

namespace PolarisGateway.Mappers
{
    public class ReclassifyDocumentRequestMapper : IReclassifyDocumentRequestMapper
    {
        public ReclassifyDocumentDto Map(DocumentReclassificationRequestDto documentReclassificationRequest)
        {
            return new ReclassifyDocumentDto
            {
                DocumentId = documentReclassificationRequest.DocumentId,
                DocumentTypeId = documentReclassificationRequest.DocumentTypeId,
                ReclassificationType = documentReclassificationRequest.ReclassificationType,
                Exhibit = documentReclassificationRequest.Exhibit,
                Statement = documentReclassificationRequest.Statement,
                Used = documentReclassificationRequest.Used
            };
        }
    }
}