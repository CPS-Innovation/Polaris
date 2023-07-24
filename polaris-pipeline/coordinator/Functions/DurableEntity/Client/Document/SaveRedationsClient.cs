using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Clients.Contracts;
using Common.Configuration;
using Common.Constants;
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
using Common.ValueObjects;
using Common.Services.BlobStorageService.Contracts;
using System.IO.Compression;

namespace coordinator.Functions.DurableEntity.Client.Document
{
    public class SaveRedactionsClient : BaseClient
    {
        const string loggingName = $"{nameof(SaveRedactionsClient)} - {nameof(HttpStart)}";

        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidator<RedactPdfRequestDto> _requestValidator;
        private readonly IPdfGeneratorClient _redactionClient;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IDdeiClient _ddeiClient;

        public SaveRedactionsClient(IJsonConvertWrapper jsonConvertWrapper,
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

        [FunctionName(nameof(SaveRedactionsClient))]
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
                #region Validate-Inputs
                var response = await GetTrackerDocument(req, client, loggingName, caseId, new PolarisDocumentId(polarisDocumentId), log);

                if (!response.Success)
                    return response.Error;

                currentCorrelationId = response.CorrelationId;
                var document = response.CmsDocument;
                var content = await req.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new BadRequestException("Request body cannot be null.", nameof(req));
                }
                var redactPdfRequest = _jsonConvertWrapper.DeserializeObject<RedactPdfRequestDto>(content);

                redactPdfRequest.FileName = document.PdfBlobName;
                var validationResult = await _requestValidator.ValidateAsync(redactPdfRequest);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(redactPdfRequest));
                #endregion

                var redactionResult = await _redactionClient.RedactPdfAsync(redactPdfRequest, currentCorrelationId);
                if (!redactionResult.Succeeded)
                {
                    string error = $"Error Saving redaction details to the document for {caseId}, polarisDocumentId {polarisDocumentId}";
                    log.LogMethodFlow(currentCorrelationId, loggingName, error);
                    throw new ArgumentException(error);
                }

                var pdfStream = await _blobStorageService.GetDocumentAsync(redactionResult.RedactedDocumentName, currentCorrelationId);
                log.LogFileStream($"GetDocument-PUT-{nameof(SaveRedactionsClient)}", redactionResult.RedactedDocumentName.Replace("/", "-"), "PDF", pdfStream);

                var cmsAuthValues = req.Headers.GetValues(HttpHeaderKeys.CmsAuthValues).FirstOrDefault();
                if (string.IsNullOrEmpty(cmsAuthValues))
                {
                    log.LogMethodFlow(currentCorrelationId, loggingName, $"No authentication header values specified");
                    throw new ArgumentException(HttpHeaderKeys.CmsAuthValues);
                }

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
                await _ddeiClient.UploadPdf(arg, pdfStream);
                log.LogFileStream($"DdieClientUploadPdf-PUT-{nameof(SaveRedactionsClient)}", redactionResult.RedactedDocumentName.Replace("/", "-"), "PDF", pdfStream);

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
