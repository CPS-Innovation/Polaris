using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Extensions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class PolarisPipelineBulkRedactionSearchPost : BaseFunction
{
    private readonly ILogger<PolarisPipelineBulkRedactionSearchPost> _logger;
    private readonly ICoordinatorClient _coordinatorClient;

    public PolarisPipelineBulkRedactionSearchPost(ILogger<PolarisPipelineBulkRedactionSearchPost> logger, ICoordinatorClient coordinatorClient)
    {
        _logger = logger;
        _coordinatorClient = coordinatorClient;
    }

    [Function(nameof(PolarisPipelineBulkRedactionSearchPost))]
    [OpenApiOperation(operationId: nameof(PolarisPipelineBulkRedactionSearchPost), tags: ["Documents"], Summary = "Orchestration Status", Description = "Schedule new orchestration instance or returns status")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(long), Description = "The document Id (version) of the material", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Orchestration Status", Description = "Schedule new orchestration instance or returns status")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.OcrSearch)] HttpRequest req, string caseUrn, int caseId, string materialId, long documentId, CancellationToken cancellationToken)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        return await (await _coordinatorClient.BulkRedactionSearchAsyncPost(caseUrn, caseId, materialId, documentId, correlationId, cmsAuthValues, cancellationToken)).ToActionResult();
    }
}
