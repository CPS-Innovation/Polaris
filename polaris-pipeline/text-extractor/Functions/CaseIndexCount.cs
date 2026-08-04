// <copyright file="CaseIndexCount.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

using Common.Configuration;
using Common.Extensions;
using Common.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using text_extractor.Services.CaseSearchService;

namespace text_extractor.Functions
{
    /// <summary>
    /// Represents a function that retrieves the count of indexed entries for a specific case based on the provided case ID. 
    /// This function is designed to be triggered by an HTTP GET request and is intended to be accessed via the Housekeeping UI front-end.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CaseIndexCount"/> class.
    /// </remarks>
    /// <param name="logger">The logger instance used to log information and errors.</param>
    /// <param name="searchIndexService">The service used to get case index count for a specified case.</param>
    /// <param name="exceptionHandler">Handler to manage exceptions.</param>
    public class CaseIndexCount(ILogger<CaseIndexCount> logger,
        ISearchIndexService searchIndexService,
        IExceptionHandler exceptionHandler) : BaseFunction
    {
        private readonly ILogger<CaseIndexCount> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ISearchIndexService searchIndexService = searchIndexService ?? throw new ArgumentNullException(nameof(searchIndexService));
        private readonly IExceptionHandler exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        private const string LoggingName = nameof(CaseIndexCount);

        [Function(nameof(CaseIndexCount))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseIndexCount)] HttpRequest request, int caseId)
        {
            var correlationId = request.Headers.GetCorrelationId();

            try
            {
                var result = await searchIndexService.GetCaseIndexCount(caseId, correlationId);

                return CreateJsonResult(result);
            }
            catch (Exception exception)
            {
                return exceptionHandler.HandleExceptionNew(exception, correlationId, LoggingName, logger);
            }
        }
    }
}