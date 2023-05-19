using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Services.SasGeneratorService;
using Common.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;

namespace coordinator.Functions.DurableEntity.Client.Document
{
    public class GetDocumentSasUrlClient : BaseClient
    {
        private readonly ISasGeneratorService _sasGeneratorService;

        const string loggingName = $"{nameof(GetDocumentSasUrlClient)} - {nameof(HttpStart)}";

        public GetDocumentSasUrlClient(ISasGeneratorService sasGeneratorService)
        {
            _sasGeneratorService = sasGeneratorService;
        }

        [FunctionName(nameof(GetDocumentSasUrlClient))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = RestApi.DocumentSasUrl)] HttpRequestMessage req,
            string caseUrn,
            string caseId,
            Guid documentId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            Guid currentCorrelationId = default;

            try
            {
                var response = await GetTrackerDocument(req, client, loggingName, caseId, documentId, log);

                if (!response.Success)
                    return response.Error;

                currentCorrelationId = response.CorrelationId;
                var blobName = response.CmsDocument.PdfBlobName;
                var sasUrl = await _sasGeneratorService.GenerateSasUrlAsync(blobName, currentCorrelationId);

                return !string.IsNullOrEmpty(sasUrl) ? new OkObjectResult(sasUrl) : null;
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, loggingName, ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
