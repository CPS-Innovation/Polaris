using Common.Dto.Request;

namespace PolarisGateway.Mappers
{
    public class DocumentNoteRequestMapper : IDocumentNoteRequestMapper
    {
        public AddDocumentNoteDto Map(AddDocumentNoteRequestDto addDocumentNoteRequestDto)
        {
            return new AddDocumentNoteDto
            {
                DocumentId = addDocumentNoteRequestDto.DocumentId,
                Text = addDocumentNoteRequestDto.Text
            };
        }
    }
}