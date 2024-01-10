using System.Collections.Generic;
using System.Linq;
using polaris_common.Dto.Request;

namespace pdf_generator.Services.DocumentRedaction.Aspose
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
