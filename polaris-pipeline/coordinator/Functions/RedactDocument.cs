using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using coordinator.Clients.PdfRedactor;
using coordinator.Factories.UploadFileNameFactory;
using coordinator.Helpers;
using Common.Configuration;
using Common.Dto.Request;
using Common.Exceptions;
using Common.Extensions;
using Common.Services.BlobStorage;
using Common.Wrappers;
using Ddei.Factories;
using Ddei;
using FluentValidation;

namespace coordinator.Functions
{
    public class RedactDocument : BaseClient
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidator<RedactPdfRequestWithDocumentDto> _requestValidator;
        private readonly IPdfRedactorClient _redactionClient;
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly IUploadFileNameFactory _uploadFileNameFactory;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly ILogger<RedactDocument> _logger;

        public RedactDocument(IJsonConvertWrapper jsonConvertWrapper,
                              IValidator<RedactPdfRequestWithDocumentDto> requestValidator,
                              IPdfRedactorClient redactionClient,
                              IPolarisBlobStorageService polarisBlobStorageService,
                              IUploadFileNameFactory uploadFileNameFactory,
                              IDdeiClient ddeiClient,
                              IDdeiArgFactory ddeiArgFactory,
                              ILogger<RedactDocument> logger)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _requestValidator = requestValidator;
            _redactionClient = redactionClient;
            _polarisBlobStorageService = polarisBlobStorageService;
            _uploadFileNameFactory = uploadFileNameFactory;
            _ddeiClient = ddeiClient;
            _ddeiArgFactory = ddeiArgFactory;
            _logger = logger;
        }

        [FunctionName(nameof(RedactDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RedactDocument)]
            HttpRequestMessage req,
            string caseUrn,
            int caseId,
            string documentId,
            [DurableClient] IDurableEntityClient client)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                var response = await GetTrackerDocument(client, caseId, documentId, _logger, currentCorrelationId, nameof(RedactDocument));
                var document = response.CmsDocument;

                var content = await req.Content.ReadAsStringAsync();
                var redactPdfRequest = _jsonConvertWrapper.DeserializeObject<RedactPdfRequestDto>(content);

                using var documentStream = await _polarisBlobStorageService.GetBlobAsync(new BlobIdType(caseId, documentId, document.VersionId, BlobType.Pdf));

                using var memoryStream = new MemoryStream();
                await documentStream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                Stream redactedDocumentStream = null;

                if (redactPdfRequest.RedactionDefinitions.Any())
                {
                    var base64Document = Convert.ToBase64String(bytes);

                    var redactionRequest = new RedactPdfRequestWithDocumentDto
                    {
                        Document = base64Document,
                        RedactionDefinitions = redactPdfRequest.RedactionDefinitions,
                        VersionId = redactPdfRequest.VersionId
                    };

                    var validationResult = await _requestValidator.ValidateAsync(redactionRequest);
                    if (!validationResult.IsValid)
                        throw new BadRequestException(validationResult.FlattenErrors(), nameof(redactPdfRequest));

                    redactedDocumentStream = await _redactionClient.RedactPdfAsync(caseUrn, caseId, documentId, redactionRequest, currentCorrelationId);
                    if (redactedDocumentStream == null)
                    {
                        string error = $"Error Saving redaction details to the document for {caseId}, documentId {documentId}";
                        throw new Exception(error);
                    }
                }

                Stream modifiedDocumentStream = null;

                if (redactPdfRequest.DocumentModifications.Any())
                {
                    byte[] bytesToModify = null;

                    if (redactedDocumentStream != null)
                    {
                        using var redactedMemoryStream = new MemoryStream();
                        await redactedDocumentStream.CopyToAsync(redactedMemoryStream);
                        bytesToModify = redactedMemoryStream.ToArray();
                    }
                    else
                    {
                        bytesToModify = bytes;
                    }

                    var base64DocumentToModify = Convert.ToBase64String(bytesToModify);

                    var modificationRequest = new ModifyDocumentWithDocumentDto
                    {
                        Document = base64DocumentToModify,
                        DocumentModifications = redactPdfRequest.DocumentModifications,
                        VersionId = redactPdfRequest.VersionId
                    };

                    modifiedDocumentStream = await _redactionClient.ModifyDocument(caseUrn, caseId, documentId, modificationRequest, currentCorrelationId);
                    if (modifiedDocumentStream == null)
                    {
                        string error = $"Error modifying document for {caseId}, documentId {documentId}";
                        throw new Exception(error);
                    }
                }

                var cmsAuthValues = req.Headers.GetCmsAuthValues();
                var arg = _ddeiArgFactory.CreateDocumentArgDto
                (
                    cmsAuthValues: cmsAuthValues,
                    correlationId: currentCorrelationId,
                    urn: caseUrn,
                    caseId: caseId,
                    documentId: document.CmsDocumentId,
                    versionId: document.VersionId
                );

                var ddeiResult = await _ddeiClient.UploadPdfAsync(arg, modifiedDocumentStream ?? redactedDocumentStream);

                if (ddeiResult.StatusCode == HttpStatusCode.Gone || ddeiResult.StatusCode == HttpStatusCode.RequestEntityTooLarge)
                {
                    return new StatusCodeResult((int)ddeiResult.StatusCode);
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(RedactDocument), currentCorrelationId, ex);
            }
        }
    }
}
