using Common.Dto.Document;

namespace Common.Mappers.Contracts
{
    public interface ITransitionDocumentMapper
    {
        TransitionDocumentDto MapToTransitionDocument(DocumentDto cmsCaseDocument);
    }
}