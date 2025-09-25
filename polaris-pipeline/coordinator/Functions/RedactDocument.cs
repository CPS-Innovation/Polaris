using Common.Configuration;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Exceptions;
using Common.Extensions;
using Common.Services.BlobStorage;
using coordinator.Clients.PdfRedactor;
using DdeiClient.Clients.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DdeiClient.Factories;

namespace coordinator.Functions;

public class RedactDocument
{
    private readonly IValidator<RedactPdfRequestWithDocumentDto> _requestValidator;
    private readonly IPdfRedactorClient _redactionClient;
    private readonly IPolarisBlobStorageService _polarisBlobStorageService;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IMdsClient _mdsClient;
    private readonly ILogger<RedactDocument> _logger;

    public RedactDocument(
        IValidator<RedactPdfRequestWithDocumentDto> requestValidator,
        IPdfRedactorClient redactionClient,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IDdeiArgFactory ddeiArgFactory,
        ILogger<RedactDocument> logger,
        IConfiguration configuration, 
        IMdsClient mdsClient)
    {
        _requestValidator = requestValidator.ExceptionIfNull();
        _redactionClient = redactionClient.ExceptionIfNull();
        _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty).ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _logger = logger.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    [Function(nameof(RedactDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RedactDocument)]
        HttpRequest req,
        string caseUrn,
        int caseId,
        string documentId,
        long versionId)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();

        var redactPdfRequest = await req.ReadFromJsonAsync<RedactPdfRequestDto>();

        using var documentStream = await _polarisBlobStorageService.GetBlobAsync(new BlobIdType(caseId, documentId, versionId, BlobType.Pdf));

        using var memoryStream = new MemoryStream();
        await documentStream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        Stream redactedDocumentStream = null;

        if (redactPdfRequest.RedactionDefinitions.Count != 0)
        {
            var base64Document = Convert.ToBase64String(bytes);

            var redactionRequest = new RedactPdfRequestWithDocumentDto
            {
                Document = base64Document,
                RedactionDefinitions = redactPdfRequest.RedactionDefinitions,
            };

            var validationResult = await _requestValidator.ValidateAsync(redactionRequest);
            if (!validationResult.IsValid)
            {
                throw new BadRequestException(validationResult.FlattenErrors(), nameof(redactPdfRequest));
            }

            redactedDocumentStream = await _redactionClient.RedactPdfAsync(caseUrn, caseId, documentId, versionId, redactionRequest, currentCorrelationId);
            if (redactedDocumentStream == null)
            {
                string error = $"Error Saving redaction details to the document for {caseId}, documentId {documentId}";
                throw new Exception(error);
            }
        }

        Stream modifiedDocumentStream = null;

        if (redactPdfRequest.DocumentModifications.Count != 0)
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

            modifiedDocumentStream = await _redactionClient.ModifyDocument(caseUrn, caseId, documentId, versionId, modificationRequest, currentCorrelationId);
            if (modifiedDocumentStream == null)
            {
                string error = $"Error modifying document for {caseId}, documentId {documentId}";
                throw new Exception(error);
            }
        }

        var cmsAuthValues = req.Headers.GetCmsAuthValues();
        var arg = _ddeiArgFactory.CreateDocumentVersionArgDto(
            cmsAuthValues,
            correlationId: currentCorrelationId,
            caseUrn,
            caseId: caseId,
            DocumentNature.ToNumericDocumentId(documentId, DocumentNature.Types.Document),
            versionId);

            
        var ddeiResult = await _mdsClient.UploadPdfAsync(arg, modifiedDocumentStream ?? redactedDocumentStream);

        if (ddeiResult.StatusCode == HttpStatusCode.Gone || ddeiResult.StatusCode == HttpStatusCode.RequestEntityTooLarge)
        {
            return new StatusCodeResult((int)ddeiResult.StatusCode);
        }

        return new OkResult();
    }
}