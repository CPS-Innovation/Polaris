// <copyright file="GetExhibitProducers.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions.HouseKeeping;

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Functions;
using PolarisGateway.Helpers;

/// <summary>
/// Represents a function that retrieves exhibit producers for a given case.
/// </summary>
/// <param name="logger">The logger instance used to log warnings and errors.</param>
/// <param name="communicationService">The service called to get producers.</param>
/// <param name="witnessService">The service called to return case witnesses.</param>
public class GetExhibitProducers(ILogger<GetExhibitProducers> logger,
    ICommunicationService communicationService,
    IWitnessService witnessService) : BaseFunction(logger)
{
    private readonly ILogger<GetExhibitProducers> logger = logger;
    private readonly ICommunicationService communicationService = communicationService;
    private readonly IWitnessService witnessService = witnessService;

    /// <summary>
    /// The Azure Function that processes an HTTP request for the 'api/exhibit-producers' route.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="caseId">The case Id.</param>
    /// <returns>An <see cref="IActionResult"/> The response of the function.</returns>
    [OpenApiOperation(operationId: nameof(GetExhibitProducers), tags: ["ExhibitProducer"], Description = "Represents a function that retrieves exhibit producers for a case.")]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header, Description = "The Azure Function API Key.")]
    [OpenApiSecurity("Cookie", SecuritySchemeType.ApiKey, Name = "Cookie", In = OpenApiSecurityLocationType.Header, Description = "The CMS Auth Values. This can be retrieved via the DDEI Authenticate API Endpoint and URI encoded along with User session token.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ExhibitProducersResponse), Description = "Return success response with body.")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.UnprocessableEntity)]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized)]
    [Function(nameof(GetExhibitProducers))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.ExhibitProducers)] HttpRequest request, int caseId)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} {nameof(GetExhibitProducers)} function processed a request.");

            if (caseId < 1)
            {
                return new BadRequestObjectResult($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
            }

            // Build CMS auth values from cookie extracted from the request
            var cmsAuthValues = this.BuildCmsAuthValues(request);

            var exhibitTask = this.communicationService.GetExhibitProducersAsync(caseId, cmsAuthValues);
            var witnessTask = this.witnessService.GetCaseWitnessesAsync(caseId, cmsAuthValues);

            await Task.WhenAll(exhibitTask, witnessTask).ConfigureAwait(false);

            var result = await exhibitTask.ConfigureAwait(false);
            var witnesses = (await witnessTask.ConfigureAwait(false))?.Witnesses;

            if (witnesses?.Any() == true)
            {
                result ??= new ExhibitProducersResponse();
                result.ExhibitProducers ??= [];

                result.ExhibitProducers.AddRange(
                    witnesses.Select(w =>
                        new ExhibitProducer(w.WitnessId!.Value, GetFullName(w))));
            }

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] {nameof(GetExhibitProducers)} function completed in [{stopwatch.Elapsed}]");

            // Set both cache and security headers
            ResponseHeaderHelper.SetNoCacheHeaders(request.HttpContext.Response);
            ResponseHeaderHelper.SetSecurityHeaders(request.HttpContext.Response);

            return new OkObjectResult(result);
        }
        catch (InvalidOperationException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} {nameof(GetExhibitProducers)} function encountered an invalid operation error: {ex.Message}");
            return new UnprocessableEntityObjectResult($"{ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} {nameof(GetExhibitProducers)} function encountered an unauthorized access error: {ex.Message}");
            return new UnauthorizedObjectResult($"{nameof(GetExhibitProducers)} error: {ex.Message}");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} {nameof(GetExhibitProducers)} function encountered an error: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get witness full name.
    /// </summary>
    /// <param name="witness">The witness.</param>
    /// <returns>Full name.</returns>
    private static string GetFullName(Witness witness)
    {
        return string.Join(" ", new[] { witness.FirstName, witness.Surname }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }
}
