
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using pdf_thumbnail_generator.Durable.Activity;
using pdf_thumbnail_generator.Durable.Payloads;

namespace pdf_thumbnail_generator.Durable.Orchestration
{
  public class RepresentativeThumbnailOrchestrator
  {
    private readonly ILogger<RepresentativeThumbnailOrchestrator> _logger;

    public RepresentativeThumbnailOrchestrator(ILogger<RepresentativeThumbnailOrchestrator> logger)
    {
      _logger = logger;
    }

    [Function(nameof(RepresentativeThumbnailOrchestrator))]
    public async Task<List<string>> Run([OrchestrationTrigger] TaskOrchestrationContext context)
    {
      var payload = context.GetInput<RepresentativeThumbnailOrchestrationPayload>()
          ?? throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

      // _logger.LogInformation("Saying hello.");
      var outputs = new List<string>();

      // // Replace name and input with values relevant for your Durable Functions Activity
      outputs.Add(await context.CallActivityAsync<string>(nameof(RepresentativeThumbnail), payload));
      // outputs.Add(await context.CallActivityAsync<string>(nameof(GenerateRepresentativeThumbnail), "Seattle"));
      // outputs.Add(await context.CallActivityAsync<string>(nameof(GenerateRepresentativeThumbnail), "London"));

      // // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
      return outputs;
    }
  }
}