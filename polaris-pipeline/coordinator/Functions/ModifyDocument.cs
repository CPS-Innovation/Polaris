using System;
using System.IO;
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
using Common.Wrappers;
using Ddei.Factories;
using DdeiClient;
using FluentValidation;

namespace coordinator.Functions
{
    public class ModifyDocument : BaseClient
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidator<ModifyDocumentWithDocumentDto> _requestValidator;
        private readonly IPdfRedactorClient _pdfRedactorClient;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IUploadFileNameFactory _uploadFileNameFactory;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly ILogger<ModifyDocument> _logger;

        public ModifyDocument(
            IJsonConvertWrapper jsonConvertWrapper,
            IValidator<ModifyDocumentWithDocumentDto> requestValidator,
            IPdfRedactorClient pdfRedactorClient,
            IPolarisBlobStorageService blobStorageService,
            IUploadFileNameFactory uploadFileNameFactory,
            IDdeiClient ddeiClient,
            IDdeiArgFactory ddeiArgFactory,
            ILogger<ModifyDocument> logger)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _requestValidator = requestValidator;
            _pdfRedactorClient = pdfRedactorClient;
            _blobStorageService = blobStorageService;
            _uploadFileNameFactory = uploadFileNameFactory;
            _ddeiClient = ddeiClient;
            _ddeiArgFactory = ddeiArgFactory;
            _logger = logger;
        }

        [FunctionName(nameof(ModifyDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ModifyDocument)]
            HttpRequestMessage req,
            string caseUrn,
            string caseId,
            string documentId,
            [DurableClient] IDurableEntityClient client)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                var response = await GetTrackerDocument(client, caseId, documentId, _logger, currentCorrelationId, nameof(ModifyDocument));
                var document = response.CmsDocument;

                var content = await req.Content.ReadAsStringAsync();
                var modifyDocumentRequest = _jsonConvertWrapper.DeserializeObject<ModifyDocumentRequestDto>(content);

                using var documentStream = await _blobStorageService.GetDocumentAsync(document.PdfBlobName, currentCorrelationId);

                using var memoryStream = new MemoryStream();
                await documentStream.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                var base64Document = Convert.ToBase64String(bytes);

                var modificationRequest = new ModifyDocumentWithDocumentDto
                {
                    Document = base64Document,
                    FileName = document.PdfBlobName,
                    DocumentModifications = modifyDocumentRequest.DocumentModifications,
                    VersionId = modifyDocumentRequest.VersionId
                };

                var validationResult = await _requestValidator.ValidateAsync(modificationRequest);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(modificationRequest));

                using var modifiedDocumentStream = await _pdfRedactorClient.ModifyDocument(caseUrn, caseId, documentId, modificationRequest, currentCorrelationId);
                if (modifiedDocumentStream == null)
                {
                    string error = $"Error modifying document for {caseId}, documentId {documentId}";
                    throw new Exception(error);
                }

                var uploadFileName = _uploadFileNameFactory.BuildUploadFileName(document.PdfBlobName);

                await _blobStorageService.UploadDocumentAsync(
                    modifiedDocumentStream,
                    uploadFileName,
                    caseId,
                    documentId,
                    modificationRequest.VersionId.ToString(),
                    currentCorrelationId);

                using var pdfStream = await _blobStorageService.GetDocumentAsync(uploadFileName, currentCorrelationId);

                var cmsAuthValues = req.Headers.GetCmsAuthValues();
                var arg = _ddeiArgFactory.CreateDocumentArgDto
                (
                    cmsAuthValues: cmsAuthValues,
                    correlationId: currentCorrelationId,
                    urn: caseUrn,
                    caseId: int.Parse(caseId),
                    documentId: document.CmsDocumentId,
                    versionId: document.VersionId
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
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(ModifyDocument), currentCorrelationId, ex);
            }
        }
    }
}