using Common.Dto.Response.Document;
using Ddei.Domain.Response.Document;

namespace Ddei.Mappers
{
    public class CaseDocumentNoteMapper : ICaseDocumentNoteMapper
    {
        public DocumentNoteDto Map(DdeiDocumentNoteResponse ddeiResponse)
        {
            return new DocumentNoteDto
            {
                Id = ddeiResponse.Id,
                CreatedByName = ddeiResponse.CreatedByName,
                Date = ddeiResponse.Date,
                SortOrder = int.Parse(ddeiResponse.Number),
                Text = ddeiResponse.Text,
                Type = ddeiResponse.Type
            };
        }
    }
}