using Common.Dto.Response.Document;
using Ddei.Domain.Response.Document;

namespace Ddei.Mappers
{
    public class CaseDocumentNoteResultMapper : ICaseDocumentNoteResultMapper
    {
        public DocumentNoteResult Map(DdeiDocumentNoteAddedResponse ddeiResponse)
        {
            return new DocumentNoteResult { Id = ddeiResponse.Id };
        }
    }
}