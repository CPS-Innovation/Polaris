using Common.Dto.Request;

namespace PolarisGateway.Mappers
{
    public class ModifyDocumentRequestMapper : IModifyDocumentRequestMapper
    {
        public ModifyDocumentDto Map(DocumentModificationRequestDto documentModificationRequest)
        {
            return new ModifyDocumentDto
            {
                DocumentChanges = documentModificationRequest.DocumentChanges
            };
        }
    }
}
