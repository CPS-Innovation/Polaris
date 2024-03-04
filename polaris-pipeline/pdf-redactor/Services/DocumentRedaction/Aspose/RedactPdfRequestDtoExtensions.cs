using System.Collections.Generic;
using System.Linq;
using Common.Dto.Request;

namespace pdf_redactor.Services.DocumentRedaction.Aspose
{
  public static class RedactPdfRequestDtoExtensions
  {
    public static Dictionary<int, int> RedactionPageCounts(this RedactPdfRequestDto redactPdfRequestDto)
    {
      return redactPdfRequestDto
        .RedactionDefinitions
        .ToDictionary(r => r.PageIndex, r => r.RedactionCoordinates.Count);
    }
  }
}
