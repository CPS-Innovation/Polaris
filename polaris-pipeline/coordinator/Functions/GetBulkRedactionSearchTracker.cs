using Common.Configuration;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace coordinator.Functions;

public class GetBulkRedactionSearchTracker
{
    private readonly ICaseDurableEntityMapper _caseDurableEntityMapper;
    private readonly IStateStorageService _stateStorageService;
    private readonly ILogger<GetTracker> _logger;
    private const string SearchTextHeader = "SearchText";

    public GetBulkRedactionSearchTracker(ICaseDurableEntityMapper caseDurableEntityMapper,
        IStateStorageService stateStorageService,
        ILogger<GetTracker> logger)
    {
        _caseDurableEntityMapper = caseDurableEntityMapper;
        _stateStorageService = stateStorageService;
        _logger = logger;
    }

    [Function(nameof(GetBulkRedactionSearchTracker))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.OcrSearchTracker)] HttpRequest req,
        string caseUrn, int caseId, string documentId, long versionId)
    {
        var searchText = req.Query[SearchTextHeader];
        try
        {
            var caseEntity = await _stateStorageService.GetBulkRedactionSearchStateAsync(caseId, documentId, versionId, searchText);
            return new OkObjectResult(caseEntity);
        }
        catch (Exception ex)
        {
            return new NotFoundObjectResult($"No Case Entity found with id '{caseId}' with exception '{ex.GetType().Name}: {ex.Message}");
        }
    }
}