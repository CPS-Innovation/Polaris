// <copyright file="RemoveCaseIndexes.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

using Common.Configuration;
using Common.Extensions;
using Common.Handlers;
using text_extractor.Services.CaseSearchService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace text_extractor.Functions
{
    /// <summary>
    /// Represents a function that removes indexed entries for a specific case based on the provided case ID.
    /// This function is designed to be triggered by an HTTP POST request and is intended to be accessed via the Housekeeping UI front-end.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RemoveCaseIndexes"/> class.
    /// </remarks>
    /// <param name="logger">The logger instance used to log information and errors.</param>
    /// <param name="searchIndexService">The service used to remove case index count for a specified case.</param>
    /// <param name="exceptionHandler">Handler to manage exceptions.</param>
    public class RemoveCaseIndexes(
        ILogger<RemoveCaseIndexes> logger,
        ISearchIndexService searchIndexService,
        IExceptionHandler exceptionHandler) : BaseFunction
    {
        private readonly ILogger<RemoveCaseIndexes> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ISearchIndexService searchIndexService = searchIndexService ?? throw new ArgumentNullException(nameof(searchIndexService));
        private readonly IExceptionHandler exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        private const string LoggingName = nameof(RemoveCaseIndexes);

        [Function(nameof(RemoveCaseIndexes))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.RemoveCaseIndexes)] HttpRequest request, int caseId)
        {
            Guid correlationId = request.Headers.GetCorrelationId();

            try
            {
                var result = await searchIndexService.RemoveCaseIndexEntriesAsync(caseId, correlationId);

                return CreateJsonResult(result);
            }
            catch (Exception exception)
            {
                return exceptionHandler.HandleExceptionNew(exception, correlationId, LoggingName, logger);
            }
        }
    }
}