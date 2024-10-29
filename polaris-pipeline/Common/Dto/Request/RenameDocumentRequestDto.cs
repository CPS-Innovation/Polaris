using System.ComponentModel.DataAnnotations;

namespace Common.Dto.Request
{
    public class RenameDocumentRequestDto
    {
        [Required]
        public string DocumentName { get; set; }
    }
}