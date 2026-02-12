using Common.Dto.Response.Document;
using Ddei.Domain.Response.Document;

namespace Ddei.Mappers
{
    public class CaseDocumentNoteResultMapper : ICaseDocumentNoteResultMapper
    {
        public DocumentNoteResult Map(MdsDocumentNoteAddedResponse mdsResponse)
        {
            return new DocumentNoteResult { Id = mdsResponse.Id };
        }
    }
}