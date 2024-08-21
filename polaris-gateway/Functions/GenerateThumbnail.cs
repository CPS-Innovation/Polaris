using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using Common.ValueObjects;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Handlers;
using Newtonsoft.Json;
using Common.Dto.Request;

namespace PolarisGateway.Functions
{
  public class GenerateThumbnail
  {
    private readonly ILogger<GenerateThumbnail> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private readonly IInitializationHandler _initializationHandler;
    private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

    public GenerateThumbnail(
        ILogger<GenerateThumbnail> logger,
        ICoordinatorClient coordinatorClient,
        IInitializationHandler initializationHandler,
        IUnhandledExceptionHandler unhandledExceptionHandler)
    {
      _logger = logger;
      _coordinatorClient = coordinatorClient;
      _initializationHandler = initializationHandler;
      _unhandledExceptionHandler = unhandledExceptionHandler;
    }

    [FunctionName(nameof(GenerateThumbnail))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<HttpResponseMessage> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.GenerateThumbnail)] HttpRequest req, string caseUrn, int caseId, string documentId)
    {
      (Guid CorrelationId, string CmsAuthValues) context = default;
      try
      {
        context = await _initializationHandler.Initialize(req);

        var content = await req.ReadAsStringAsync();
        var generateThumbnailRequest = JsonConvert.DeserializeObject<GenerateThumbnailRequestDto>(content);

        return await _coordinatorClient.GenerateThumbnail(
            caseUrn,
            caseId,
            new PolarisDocumentId(documentId),
            generateThumbnailRequest,
            context.CmsAuthValues,
            context.CorrelationId);

      }
      catch (Exception ex)
      {
        return _unhandledExceptionHandler.HandleUnhandledException(
          _logger,
          nameof(PolarisPipelineGetDocument),
          context.CorrelationId,
          ex
        );
      }
    }
  }
}

