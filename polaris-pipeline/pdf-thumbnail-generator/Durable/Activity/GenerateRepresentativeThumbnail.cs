using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace pdf_thumbnail_generator.Durable.Activity
{
  public class GenerateRepresentativeThumbnail
  {
    private readonly ILogger<GenerateRepresentativeThumbnail> _logger;

    public GenerateRepresentativeThumbnail(ILogger<GenerateRepresentativeThumbnail> logger)
    {
      _logger = logger;
    }

    [Function(nameof(GenerateRepresentativeThumbnail))]
    public async Task<string> Run([ActivityTrigger] string name)
    {
      _logger.LogInformation("Saying hello to {name}.", name);
      return $"Hello {name}!";
    }
  }
}