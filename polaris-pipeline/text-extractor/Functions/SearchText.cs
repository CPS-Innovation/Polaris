// <copyright file="ConvertToPdf.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

using Common.Configuration;
using Common.Dto.Request.Search;
using Common.Exceptions;
using Common.Extensions;
using Common.Handlers;
using Common.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using text_extractor.Services.CaseSearchService;

namespace text_extractor.Functions
{
    /// <summary>
    /// Represents a function that searches for text within a case material based on the provided search term.
    /// This function is designed to be triggered by an HTTP POST request and is intended to be accessed via the Housekeeping UI front-end.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="SearchText"/> class.
    /// </remarks>
    /// <param name="logger">The logger instance used to log information and errors.</param>
    /// <param name="searchIndexService">The service used to search through the case materials.</param>
    /// <param name="jsonConvertWrapper">JSON wrapper to handle serialization.</param>
    /// <param name="exceptionHandler">The handler to manage exceptions.</param>
    public class SearchText(
        ILogger<SearchText> logger,
        ISearchIndexService searchIndexService,
        IJsonConvertWrapper jsonConvertWrapper,
        IExceptionHandler exceptionHandler) : BaseFunction
    {
        private readonly ILogger<SearchText> _log = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ISearchIndexService _searchIndexService = searchIndexService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper = jsonConvertWrapper;
        private readonly IExceptionHandler _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        private const string LoggingName = nameof(SearchText);

        [Function(nameof(SearchText))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.CaseSearch)] HttpRequest request, int caseId)
        {
            var correlationId = request.Headers.GetCorrelationId();
            try
            {
                if (request.Body == null)
                {
                    throw new BadRequestException("Request body has no content", nameof(request));
                }
                var content = await request.GetRawBodyStringAsync();
                var searchDto = _jsonConvertWrapper.DeserializeObject<SearchRequestDto>(content);

                var searchResults = await _searchIndexService.QueryAsync(
                    caseId,
                    searchDto.SearchTerm);

                return CreateJsonResult(searchResults);
            }
            catch (Exception exception)
            {
                return _exceptionHandler.HandleExceptionNew(exception, correlationId, LoggingName, _log);
            }
        }
    }
}