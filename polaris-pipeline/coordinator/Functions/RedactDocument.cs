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
using Common.Services.BlobStorageService;
using Common.ValueObjects;
using Common.Wrappers;
using Ddei.Factories;
using DdeiClient.Services;
using FluentValidation;

namespace coordinator.Functions
{
    public class RedactDocument : BaseClient
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidator<RedactPdfRequestWithDocumentDto> _requestValidator;
        private readonly IPdfRedactorClient _redactionClient;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IUploadFileNameFactory _uploadFileNameFactory;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly ILogger<RedactDocument> _logger;

        public RedactDocument(IJsonConvertWrapper jsonConvertWrapper,
                              IValidator<RedactPdfRequestWithDocumentDto> requestValidator,
                              IPdfRedactorClient redactionClient,
                              IPolarisBlobStorageService blobStorageService,
                              IUploadFileNameFactory uploadFileNameFactory,
                              IDdeiClient ddeiClient,
                              IDdeiArgFactory ddeiArgFactory,
                              ILogger<RedactDocument> logger)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _requestValidator = requestValidator;
            _redactionClient = redactionClient;
            _blobStorageService = blobStorageService;
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
            string caseId,
            string polarisDocumentId,
            [DurableClient] IDurableEntityClient client)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                var response = await GetTrackerDocument(client, caseId, new PolarisDocumentId(polarisDocumentId), _logger, currentCorrelationId, nameof(RedactDocument));
                var document = response.CmsDocument;

                var content = await req.Content.ReadAsStringAsync();
                var redactPdfRequest = _jsonConvertWrapper.DeserializeObject<RedactPdfRequestDto>(content);

                using var documentStream = await _blobStorageService.GetDocumentAsync(document.PdfBlobName, currentCorrelationId);

                using var memoryStream = new MemoryStream();
                await documentStream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                var base64Document = Convert.ToBase64String(bytes);

                var redactionRequest = new RedactPdfRequestWithDocumentDto
                {
                    FileName = document.PdfBlobName,
                    Document = base64Document,
                    RedactionDefinitions = redactPdfRequest.RedactionDefinitions,
                    VersionId = redactPdfRequest.VersionId
                };

                var validationResult = await _requestValidator.ValidateAsync(redactionRequest);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(redactPdfRequest));

                using var redactedDocumentStream = await _redactionClient.RedactPdfAsync(caseUrn, caseId, polarisDocumentId, redactionRequest, currentCorrelationId);
                if (redactedDocumentStream == null)
                {
                    string error = $"Error Saving redaction details to the document for {caseId}, polarisDocumentId {polarisDocumentId}";
                    throw new Exception(error);
                }

                Stream modifiedDocumentStream = null;

                if (redactPdfRequest.DocumentModifications.Any())
                {
                    using var redactedMemoryStream = new MemoryStream();
                    await redactedDocumentStream.CopyToAsync(redactedMemoryStream);
                    var redactedBytes = redactedMemoryStream.ToArray();

                    var base64RedactedDocument = Convert.ToBase64String(redactedBytes);

                    var modificationRequest = new ModifyDocumentWithDocumentDto
                    {
                        Document = base64RedactedDocument,
                        FileName = document.PdfBlobName,
                        DocumentModifications = redactPdfRequest.DocumentModifications,
                        VersionId = redactPdfRequest.VersionId
                    };

                    modifiedDocumentStream = await _redactionClient.ModifyDocument(caseUrn, caseId, polarisDocumentId, modificationRequest, currentCorrelationId);
                    if (modifiedDocumentStream == null)
                    {
                        string error = $"Error modifying document for {caseId}, polarisDocumentId {polarisDocumentId}";
                        throw new Exception(error);
                    }
                }

                var uploadFileName = _uploadFileNameFactory.BuildUploadFileName(redactionRequest.FileName);

                await _blobStorageService.UploadDocumentAsync(
                    modifiedDocumentStream ?? redactedDocumentStream,
                    uploadFileName,
                    caseId,
                    polarisDocumentId,
                    redactPdfRequest.VersionId.ToString(),
                    currentCorrelationId);

                using var pdfStream = await _blobStorageService.GetDocumentAsync(uploadFileName, currentCorrelationId);

                var cmsAuthValues = req.Headers.GetCmsAuthValues();
                var arg = _ddeiArgFactory.CreateDocumentArgDto
                (
                    cmsAuthValues: cmsAuthValues,
                    correlationId: currentCorrelationId,
                    urn: caseUrn,
                    caseId: int.Parse(caseId),
                    documentId: int.Parse(document.CmsDocumentId),
                    versionId: document.CmsVersionId
                );

                var ddeiResult = await _ddeiClient.UploadPdfAsync(arg, pdfStream);

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
