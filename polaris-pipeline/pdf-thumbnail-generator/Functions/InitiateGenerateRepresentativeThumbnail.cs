using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using pdf_thumbnail_generator.Durable.Orchestration;

namespace pdf_thumbnail_generator.Functions
{
  public class InitiateGenerateRepresentativeThumbnail
  {
    private readonly ILogger<InitiateGenerateRepresentativeThumbnail> _logger;
    private readonly DurableTaskClient _durableOrchestrationClient;

    public InitiateGenerateRepresentativeThumbnail(
      ILogger<InitiateGenerateRepresentativeThumbnail> logger,
      DurableTaskClient durableOrchestrationClient)
    {
      _logger = logger;
      _durableOrchestrationClient = durableOrchestrationClient;
    }

    [Function(nameof(InitiateGenerateRepresentativeThumbnail))]
    public async Task<HttpResponseData> Run(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req,
      FunctionContext executionContext)
    {
      ILogger logger = executionContext.GetLogger("InitiateGenerateRepresentativeThumbnail");

      string instanceId = await _durableOrchestrationClient.ScheduleNewOrchestrationInstanceAsync(
          nameof(GenerateRepresentativeThumbnailOrchestrator));

      logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

      // Returns an HTTP 202 response with an instance management payload.
      // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
      return await _durableOrchestrationClient.CreateCheckStatusResponseAsync(req, instanceId);
    }
  }
}