using System.Collections.Generic;
using Common.Dto.Request.DocumentManipulation;

namespace Common.Dto.Request
{
    public class ModifyDocumentDto
    {
        public List<DocumentChangesDto> DocumentChanges { get; set; }
    }
}