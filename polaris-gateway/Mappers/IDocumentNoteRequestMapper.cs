using Common.Dto.Request;

namespace PolarisGateway.Mappers
{
    public interface IDocumentNoteRequestMapper
    {
        AddDocumentNoteDto Map(AddDocumentNoteRequestDto addDocumentNoteRequestDto);
    }
}