using System.Collections.Generic;
using Common.Dto.Request.DocumentManipulation;

namespace Common.Dto.Request
{
    public class ModifyDocumentDto
    {
        public List<DocumentModificationDto> DocumentModifications { get; set; }
    }
}