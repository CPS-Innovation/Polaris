using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using coordinator.Clients.PdfRedactor;
using Common.Configuration;
using Common.Dto.Request;
using Common.Exceptions;
using Common.Extensions;
using Common.Services.BlobStorage;
using Common.Wrappers;
using Ddei.Factories;
using Ddei;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using Common.Domain.Document;

namespace coordinator.Functions
{
    public class ModifyDocument : BaseClient
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidator<ModifyDocumentWithDocumentDto> _requestValidator;
        private readonly IPdfRedactorClient _pdfRedactorClient;
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly ILogger<ModifyDocument> _logger;

        public ModifyDocument(
            IJsonConvertWrapper jsonConvertWrapper,
            IValidator<ModifyDocumentWithDocumentDto> requestValidator,
            IPdfRedactorClient pdfRedactorClient,
            Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
            IDdeiClient ddeiClient,
            IDdeiArgFactory ddeiArgFactory,
            ILogger<ModifyDocument> logger,
            IConfiguration configuration)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _requestValidator = requestValidator;
            _pdfRedactorClient = pdfRedactorClient;
            _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
            _ddeiClient = ddeiClient;
            _ddeiArgFactory = ddeiArgFactory;
            _logger = logger;
        }

        [Function(nameof(ModifyDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ModifyDocument)]
            HttpRequest req,
            string caseUrn,
            int caseId,
            string documentId,
            long versionId)
        {
            var currentCorrelationId = req.Headers.GetCorrelationId();

            var modifyDocumentRequest = await req.ReadFromJsonAsync<ModifyDocumentRequestDto>();

            await using var documentStream = await _polarisBlobStorageService.GetBlobAsync(new BlobIdType(caseId, documentId, versionId, BlobType.Pdf));

            using var memoryStream = new MemoryStream();
            await documentStream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            var base64Document = Convert.ToBase64String(bytes);

            var modificationRequest = new ModifyDocumentWithDocumentDto
            {
                Document = base64Document,
                DocumentModifications = modifyDocumentRequest.DocumentModifications,
                VersionId = modifyDocumentRequest.VersionId
            };

            var validationResult = await _requestValidator.ValidateAsync(modificationRequest);
            if (!validationResult.IsValid)
            {
                throw new BadRequestException(validationResult.FlattenErrors(), nameof(modificationRequest));
            }

            await using var modifiedDocumentStream = await _pdfRedactorClient.ModifyDocument(caseUrn, caseId, documentId, versionId, modificationRequest, currentCorrelationId);
            if (modifiedDocumentStream == null)
            {
                var error = $"Error modifying document for {caseId}, documentId {documentId}";
                throw new Exception(error);
            }

            var cmsAuthValues = req.Headers.GetCmsAuthValues();
            var arg = _ddeiArgFactory.CreateDocumentVersionArgDto(
                cmsAuthValues,
                currentCorrelationId,
                caseUrn,
                caseId,
                DocumentNature.ToNumericDocumentId(documentId, DocumentNature.Types.Document),
                versionId);

            var ddeiResult = await _ddeiClient.UploadPdfAsync(arg, modifiedDocumentStream);

            if (ddeiResult.StatusCode == HttpStatusCode.Gone || ddeiResult.StatusCode == HttpStatusCode.RequestEntityTooLarge)
            {
                return new StatusCodeResult((int)ddeiResult.StatusCode);
            }

            return new OkResult();
        }
    }
}