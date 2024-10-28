using Common.Dto.Response.Document;
using Ddei.Domain.Response.Document;

namespace Ddei.Mappers
{
    public interface ICaseDocumentNoteResultMapper
    {
        DocumentNoteResult Map(DdeiDocumentNoteAddedResponse ddeiResponse);
    }
}