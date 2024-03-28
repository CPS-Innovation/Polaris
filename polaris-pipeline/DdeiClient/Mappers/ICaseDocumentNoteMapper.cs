using Common.Dto.Document;
using Common.Dto.Response;

namespace Ddei.Mappers
{
    public interface ICaseDocumentNoteMapper
    {
        DocumentNoteDto Map(DdeiCaseDocumentNoteResponse ddeiResponse);
    }
}