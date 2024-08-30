using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using pdf_thumbnail_generator.Durable.Payloads;

namespace pdf_thumbnail_generator.Durable.Activity
{
  public class RepresentativeThumbnail
  {
    private readonly ILogger<RepresentativeThumbnail> _logger;

    public RepresentativeThumbnail(ILogger<RepresentativeThumbnail> logger)
    {
      _logger = logger;
    }

    [Function(nameof(RepresentativeThumbnail))]
    public async Task<string> Run([ActivityTrigger] RepresentativeThumbnailOrchestrationPayload payload)
    {

      // get the pdf from blob storage

      // generate the thumbnail for each page

      return $"{payload.CmsCaseId}!";
    }
  }
}