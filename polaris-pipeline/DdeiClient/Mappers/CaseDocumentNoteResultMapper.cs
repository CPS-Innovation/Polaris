using Common.Dto.Document;
using Common.Dto.Response;

namespace Ddei.Mappers
{
    public class CaseDocumentNoteResultMapper : ICaseDocumentNoteResultMapper
    {
        public DocumentNoteResult Map(DdeiCaseDocumentNoteAddedResponse ddeiResponse)
        {
            return new DocumentNoteResult { Id = ddeiResponse.Id };
        }
    }
}