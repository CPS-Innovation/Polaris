using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Clients.Contracts;
using Common.Configuration;
using Common.Constants;
using Common.Domain.Requests;
using Common.Logging;
using coordinator.Domain.Tracker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Domain.Extensions;
using Common.Wrappers.Contracts;
using Common.Domain.Exceptions;
using FluentValidation;
using Ddei.Domain.CaseData.Args;
using Ddei.Services;

namespace coordinator.Functions.ClientFunctions
{
    public class SaveRedactions
    {
        const string loggingName = $"{nameof(SaveRedactions)} - {nameof(HttpStart)}";
        const string correlationErrorMessage = "Invalid correlationId. A valid GUID is required.";

        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidator<RedactPdfRequest> _requestValidator;
        private readonly IRedactionClient _redactionClient;
        private readonly IBlobStorageClient _blobStorageClient;
        private readonly IDocumentService _documentService;

        public SaveRedactions(IJsonConvertWrapper jsonConvertWrapper, 
                              IValidator<RedactPdfRequest> requestValidator, 
                              IRedactionClient redactionClient, 
                              IBlobStorageClient blobStorageClient,
                              IDocumentService documentService)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _requestValidator = requestValidator;
            _redactionClient = redactionClient;
            _blobStorageClient = blobStorageClient;
            _documentService = documentService;
        }

        [FunctionName(nameof(SaveRedactions))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = RestApi.Document)] 
            HttpRequestMessage req,
            string caseUrn,
            string caseId,
            Guid documentId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            req.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
            if (correlationIdValues == null)
            {
                log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                return new BadRequestObjectResult(correlationErrorMessage);
            }

            var correlationId = correlationIdValues.FirstOrDefault();
            if (!Guid.TryParse(correlationId, out var currentCorrelationId))
                if (currentCorrelationId == Guid.Empty)
                {
                    log.LogMethodFlow(Guid.Empty, loggingName, correlationErrorMessage);
                    return new BadRequestObjectResult(correlationErrorMessage);
                }

            log.LogMethodEntry(currentCorrelationId, loggingName, caseId);

            var entityId = new EntityId(nameof(Domain.Tracker), caseId);
            var stateResponse = await client.ReadEntityStateAsync<Tracker>(entityId);
            if (!stateResponse.EntityExists)
            {
                var baseMessage = $"No pipeline tracker found with id '{caseId}'";
                log.LogMethodFlow(currentCorrelationId, loggingName, baseMessage);
                return new NotFoundObjectResult(baseMessage);
            }

            var document = stateResponse.EntityState.Documents.GetDocument(documentId);
            if (document == null)
            {
                var baseMessage = $"No document found with id '{documentId}'";
                log.LogMethodFlow(currentCorrelationId, loggingName, baseMessage);
                return new NotFoundObjectResult(baseMessage);
            }

            var content = await req.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new BadRequestException("Request body cannot be null.", nameof(req));
            }
            var redactPdfRequest = _jsonConvertWrapper.DeserializeObject<RedactPdfRequest>(content);

            redactPdfRequest.FileName = Path.ChangeExtension($"{caseId}/pdfs/{document.CmsOriginalFileName}", ".pdf");
            var validationResult = await _requestValidator.ValidateAsync(redactPdfRequest);
            if (!validationResult.IsValid)
                throw new BadRequestException(validationResult.FlattenErrors(), nameof(redactPdfRequest));

            var redactionResult = await _redactionClient.RedactPdfAsync(redactPdfRequest, currentCorrelationId);
            if (!redactionResult.Succeeded)
            {
                string error = $"Error Saving redaction details to the document for {caseId}, documentId {documentId}";
                log.LogMethodFlow(currentCorrelationId, loggingName, error);
                throw new ArgumentException(error);
            }

            // todo: trapping when blob retrieval hasn't worked
            var pdfStream = await _blobStorageClient.GetDocumentAsync(redactionResult.RedactedDocumentName, currentCorrelationId);

            await _documentService.UploadPdf(new CmsDocumentArg
            {
                Urn = caseUrn,
                CaseId = long.Parse(caseId),
                CmsDocCategory = document.CmsDocType.DocumentCategory,
                DocumentId = int.Parse(document.CmsDocumentId),
            }, pdfStream, document.CmsOriginalFileName);

            return new OkResult();

        }
    }
}
