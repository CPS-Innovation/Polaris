using System.Collections.Generic;
using Common.Dto.Request.DocumentManipulation;

namespace Common.Dto.Request
{
    public class ModifyDocumentRequestDto
    {
        public List<DocumentChangesDto> DocumentChanges { get; set; }
        public string FileName { get; set; }
        public string VersionId { get; set; }
    }
}