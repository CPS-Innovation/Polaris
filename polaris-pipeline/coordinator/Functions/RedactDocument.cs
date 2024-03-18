using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Extensions;
using Common.Wrappers;
using Common.Exceptions;
using FluentValidation;
using Common.Dto.Request;
using DdeiClient.Services;
using Common.ValueObjects;
using Common.Services.BlobStorageService;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using coordinator.Helpers;
using coordinator.Clients.PdfRedactor;
using coordinator.Factories.UploadFileNameFactory;
using System.IO;
using Common.Streaming;

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
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = RestApi.RedactDocument)]
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

                var response = await GetTrackerDocument(client, caseId, new PolarisDocumentId(polarisDocumentId));
                var document = response.CmsDocument;

                var content = await req.Content.ReadAsStringAsync();
                var redactPdfRequest = _jsonConvertWrapper.DeserializeObject<RedactPdfRequestDto>(content);

                // get blob from storage
                using var documentStream = await _blobStorageService.GetDocumentAsync(document.PdfBlobName, currentCorrelationId);
                var bytes = await documentStream.EnsureSeekableAndConvertToByteArrayAsync();

                var base64Document = Convert.ToBase64String(bytes);

                // serialize into dto base 64 encode

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

                var redactionResult = await _redactionClient.RedactPdfAsync(caseUrn, caseId, polarisDocumentId, redactionRequest, currentCorrelationId);
                if (redactionResult == null)
                {
                    string error = $"Error Saving redaction details to the document for {caseId}, polarisDocumentId {polarisDocumentId}";
                    throw new Exception(error);
                }

                var uploadFileName = _uploadFileNameFactory.BuildUploadFileName(redactionRequest.FileName);

                await _blobStorageService.UploadDocumentAsync(
                    redactionResult,
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
                     documentCategory: document.CmsDocType.DocumentCategory,
                     documentId: int.Parse(document.CmsDocumentId),
                     versionId: document.CmsVersionId
                );

                await _ddeiClient.UploadPdfAsync(arg, pdfStream);

                return new OkResult();
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(RedactDocument), currentCorrelationId, ex);
            }
        }
    }
}
