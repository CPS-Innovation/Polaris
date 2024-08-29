
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using pdf_thumbnail_generator.Durable.Activity;

namespace pdf_thumbnail_generator.Durable.Orchestration
{
  public class GenerateRepresentativeThumbnailOrchestrator
  {
    private readonly ILogger<GenerateRepresentativeThumbnailOrchestrator> _logger;

    public GenerateRepresentativeThumbnailOrchestrator(ILogger<GenerateRepresentativeThumbnailOrchestrator> logger)
    {
      _logger = logger;
    }

    [Function(nameof(GenerateRepresentativeThumbnailOrchestrator))]
    public async Task<List<string>> Run([OrchestrationTrigger] TaskOrchestrationContext context)
    {
      _logger.LogInformation("Saying hello.");
      var outputs = new List<string>();

      // Replace name and input with values relevant for your Durable Functions Activity
      outputs.Add(await context.CallActivityAsync<string>(nameof(GenerateRepresentativeThumbnail), "Tokyo"));
      outputs.Add(await context.CallActivityAsync<string>(nameof(GenerateRepresentativeThumbnail), "Seattle"));
      outputs.Add(await context.CallActivityAsync<string>(nameof(GenerateRepresentativeThumbnail), "London"));

      // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
      return outputs;
    }
  }
}