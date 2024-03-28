using Common.Dto.Document;
using Common.Dto.Response;

namespace Ddei.Mappers
{
    public class CaseDocumentNoteMapper : ICaseDocumentNoteMapper
    {
        public DocumentNoteDto Map(DdeiCaseDocumentNoteResponse ddeiResponse)
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