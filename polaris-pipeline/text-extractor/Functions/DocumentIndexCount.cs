// <copyright file="DocumentIndexCount.cs" company="TheCrownProsecutionService">
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
    /// Represents a function that retrieves the count of indexed entries for a specific document based on the provided case ID, material ID, and document ID. 
    /// This function is designed to be triggered by an HTTP GET request and is intended to be accessed via the Housekeeping UI front-end.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="DocumentIndexCount"/> class.
    /// </remarks>
    /// <param name="logger">The logger instance used to log information and errors.</param>
    /// <param name="searchIndexService">The service used to get case index count for a specified document.</param>
    /// <param name="exceptionHandler">Handler to manage exceptions.</param>
    public class DocumentIndexCount(
        ILogger<DocumentIndexCount> logger,
        ISearchIndexService searchIndexService,
        IExceptionHandler exceptionHandler) : BaseFunction
    {
        private readonly ILogger<DocumentIndexCount> logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ISearchIndexService searchIndexService = searchIndexService ?? throw new ArgumentNullException(nameof(searchIndexService));
        private readonly IExceptionHandler exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        private const string LoggingName = nameof(DocumentIndexCount);

        [Function(nameof(DocumentIndexCount))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.DocumentIndexCount)] HttpRequest request, int caseId, string materialId, long documentId)
        {
            var correlationId = request.Headers.GetCorrelationId();

            try
            {
                var result = await searchIndexService.GetDocumentIndexCount(caseId, materialId, documentId, correlationId);

                return CreateJsonResult(result);
            }
            catch (Exception exception)
            {
                return exceptionHandler.HandleExceptionNew(exception, correlationId, LoggingName, logger);
            }
        }
    }
}