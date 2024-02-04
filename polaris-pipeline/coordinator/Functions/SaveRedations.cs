using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using coordinator.Clients;
using Common.Configuration;
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
using Common.Dto.Request;
using DdeiClient.Services.Contracts;
using Common.Services.BlobStorageService.Contracts;
using Common.Extensions;
using coordinator.Durable.Entity;
using coordinator.Domain;

namespace coordinator.Functions
{
    public class SaveRedactions
    {
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidator<RedactPdfRequestDto> _requestValidator;
        private readonly IPdfGeneratorClient _redactionClient;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IDdeiClient _ddeiClient;

        public SaveRedactions(IJsonConvertWrapper jsonConvertWrapper,
                              IValidator<RedactPdfRequestDto> requestValidator,
                              IPdfGeneratorClient redactionClient,
                              IPolarisBlobStorageService blobStorageService,
                              IDdeiClient ddeiClient)
        {
            _jsonConvertWrapper = jsonConvertWrapper;
            _requestValidator = requestValidator;
            _redactionClient = redactionClient;
            _blobStorageService = blobStorageService;
            _ddeiClient = ddeiClient;
        }

        [FunctionName(nameof(SaveRedactions))]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = RestApi.Document)]
            HttpRequestMessage req,
            string caseUrn,
            string caseId,
            string polarisDocumentId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                var content = await req.Content.ReadAsStringAsync();
                var redactPdfRequest = _jsonConvertWrapper.DeserializeObject<RedactPdfRequestDto>(content);

                redactPdfRequest.FileName = PdfBlobNameHelper.GetPdfBlobName(caseId, polarisDocumentId);
                var validationResult = await _requestValidator.ValidateAsync(redactPdfRequest);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(redactPdfRequest));

                var entity = await CaseDurableEntity.GetReadOnlyEntityState(client, caseId);
                var document = entity.CmsDocuments.First(doc => doc.PolarisDocumentId.ToString() == polarisDocumentId);

                var redactionResult = await _redactionClient.RedactPdfAsync(caseUrn, caseId, polarisDocumentId, redactPdfRequest, currentCorrelationId);
                if (!redactionResult.Succeeded)
                {
                    string error = $"Error Saving redaction details to the document for {caseId}, polarisDocumentId {polarisDocumentId}";
                    throw new ArgumentException(error);
                }

                using var pdfStream = await _blobStorageService.GetDocumentAsync(redactionResult.RedactedDocumentName, currentCorrelationId);

                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                DdeiCmsDocumentArgDto arg = new DdeiCmsDocumentArgDto
                {
                    CmsAuthValues = cmsAuthValues,
                    CorrelationId = currentCorrelationId,
                    Urn = caseUrn,
                    CaseId = long.Parse(caseId),
                    CmsDocCategory = document.CmsDocType.DocumentCategory,
                    DocumentId = int.Parse(document.CmsDocumentId),
                    VersionId = document.CmsVersionId
                };
                await _ddeiClient.UploadPdfAsync(arg, pdfStream);

                return new ObjectResult(redactionResult);
            }
            catch (Exception ex)
            {
                log.LogMethodError(currentCorrelationId, nameof(SaveRedactions), ex.Message, ex);
                return new StatusCodeResult(500);
            }
        }
    }
}
