using Common.Dto.Document;
using Common.Dto.Response;

namespace Ddei.Mappers
{
    public interface ICaseDocumentNoteResultMapper
    {
        DocumentNoteResult Map(DdeiCaseDocumentNoteAddedResponse ddeiResponse);
    }
}