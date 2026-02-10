using Common.Dto.Response.Document;
using Ddei.Domain.Response.Document;

namespace Ddei.Mappers
{
    public interface ICaseDocumentNoteMapper
    {
        DocumentNoteDto Map(DdeiDocumentNoteResponse ddeiResponse);
        DocumentNoteDto Map(DocumentNoteResponse ddeiResponse);
    }
}