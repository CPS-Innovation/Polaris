using System.Collections.Generic;
using Common.Dto.Request.DocumentManipulation;

namespace Common.Dto.Request
{
    public class ModifyDocumentWithDocumentDto
    {
        public List<DocumentChangesDto> DocumentChanges { get; set; }
        public string Document { get; set; }
        public string FileName { get; set; }
        public long VersionId { get; set; }
    }
}