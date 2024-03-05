using System;
using System.Net.Http;
using System.Threading.Tasks;
using coordinator.Clients;
using Common.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Domain.Extensions;
using Common.Wrappers.Contracts;
using Common.Domain.Exceptions;
using FluentValidation;
using Common.Dto.Request;
using DdeiClient.Services;
using Common.ValueObjects;
using Common.Services.BlobStorageService.Contracts;
using Common.Extensions;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using coordinator.Helpers;

namespace coordinator.Functions
{
    public class SaveRedactions : BaseClient
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidator<RedactPdfRequestDto> _requestValidator;
        private readonly IPdfRedactorClient _redactionClient;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly ILogger<SaveRedactions> _logger;

        public SaveRedactions(IJsonConvertWrapper jsonConvertWrapper,
                              IValidator<RedactPdfRequestDto> requestValidator,
                              IPdfRedactorClient redactionClient,
                              IPolarisBlobStorageService blobStorageService,
                              IDdeiClient ddeiClient,
                              IDdeiArgFactory ddeiArgFactory,
                              ILogger<SaveRedactions> logger)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _requestValidator = requestValidator;
            _redactionClient = redactionClient;
            _blobStorageService = blobStorageService;
            _ddeiClient = ddeiClient;
            _ddeiArgFactory = ddeiArgFactory;
            _logger = logger;
        }

        [FunctionName(nameof(SaveRedactions))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status511NetworkAuthenticationRequired)]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = RestApi.Document)]
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

                redactPdfRequest.FileName = document.PdfBlobName;
                var validationResult = await _requestValidator.ValidateAsync(redactPdfRequest);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(redactPdfRequest));

                var redactionResult = await _redactionClient.RedactPdfAsync(caseUrn, caseId, polarisDocumentId, redactPdfRequest, currentCorrelationId);
                if (!redactionResult.Succeeded)
                {
                    string error = $"Error Saving redaction details to the document for {caseId}, polarisDocumentId {polarisDocumentId}";
                    throw new ArgumentException(error);
                }

                using var pdfStream = await _blobStorageService.GetDocumentAsync(redactionResult.RedactedDocumentName, currentCorrelationId);

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

                return new ObjectResult(redactionResult);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(SaveRedactions), currentCorrelationId, ex);
            }
        }
    }
}
