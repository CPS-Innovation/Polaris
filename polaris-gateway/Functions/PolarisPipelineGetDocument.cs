﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Handlers;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineGetDocument
    {
        private const string PdfContentType = "application/pdf";
        private readonly ILogger<PolarisPipelineGetDocument> _logger;
        private readonly ICoordinatorClient _coordinatorClient;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;

        public PolarisPipelineGetDocument(
            ILogger<PolarisPipelineGetDocument> logger,
            ICoordinatorClient coordinatorClient,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler)
        {
            _logger = logger;
            _coordinatorClient = coordinatorClient;
            _initializationHandler = initializationHandler;
            _unhandledExceptionHandler = unhandledExceptionHandler;
        }

        [FunctionName(nameof(PolarisPipelineGetDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Document)] HttpRequest req, string caseUrn, int caseId, string documentId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;
            try
            {
                context = await _initializationHandler.Initialize(req);
                return await _coordinatorClient.GetDocumentAsync(
                    caseUrn,
                    caseId,
                    documentId,
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

