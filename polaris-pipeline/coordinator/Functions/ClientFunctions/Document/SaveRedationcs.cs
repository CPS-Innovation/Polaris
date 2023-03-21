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

namespace coordinator.Functions.ClientFunctions.Document
{
    public class SaveRedactions : BaseClientFunction
    {
        const string loggingName = $"{nameof(SaveRedactions)} - {nameof(HttpStart)}";

        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidator<RedactPdfRequest> _requestValidator;
        private readonly IRedactionClient _redactionClient;
        private readonly IPolarisStorageClient _blobStorageClient;
        private readonly IDocumentService _documentService;

        public SaveRedactions(IJsonConvertWrapper jsonConvertWrapper,
                              IValidator<RedactPdfRequest> requestValidator,
                              IRedactionClient redactionClient,
                              IPolarisStorageClient blobStorageClient,
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
            Guid currentCorrelationId = default;

            try
            {
                var response = await GetTrackerDocument(req, client, loggingName, caseId, documentId, log);

                if (!response.Success)
                    return response.Error;

                currentCorrelationId = response.CorrelationId;
                var document = response.Document;
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

                var pdfStream = await _blobStorageClient.GetDocumentAsync(redactionResult.RedactedDocumentName, currentCorrelationId);

                var cmsAuthValues = req.Headers.GetValues(HttpHeaderKeys.CmsAuthValues).FirstOrDefault();
                if (string.IsNullOrEmpty(cmsAuthValues))
                {
                    log.LogMethodFlow(currentCorrelationId, loggingName, $"No authentication header values specified");
                    throw new ArgumentException(HttpHeaderKeys.CmsAuthValues);
                }

                CmsDocumentArg arg = new CmsDocumentArg
                {
                    CmsAuthValues = cmsAuthValues,
                    CorrelationId = currentCorrelationId,
                    Urn = caseUrn,
                    CaseId = long.Parse(caseId),
                    CmsDocCategory = document.CmsDocType.DocumentCategory,
                    DocumentId = int.Parse(document.CmsDocumentId)
                };
                await _documentService.UploadPdf(arg, pdfStream, document.CmsOriginalFileName);

                return new ObjectResult(redactionResult);
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, loggingName, ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
