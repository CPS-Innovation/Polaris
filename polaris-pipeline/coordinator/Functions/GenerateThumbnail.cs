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
using Common.ValueObjects;
using Common.Wrappers;
using Ddei.Factories;
using DdeiClient.Services;
using FluentValidation;
using coordinator.Clients.PdfGenerator;
using Common.Dto.Request.DocumentManipulation;
using Common.Constants;

namespace coordinator.Functions
{
  public class GenerateThumbnail : BaseClient
  {
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    private readonly IValidator<GenerateThumbnailWithDocumentDto> _requestValidator;
    private readonly IPdfGeneratorClient _pdfGeneratorClient;
    private readonly IPolarisBlobStorageService _blobStorageService;
    private readonly ILogger<GenerateThumbnail> _logger;

    public GenerateThumbnail(
      IJsonConvertWrapper jsonConvertWrapper,
      IValidator<GenerateThumbnailWithDocumentDto> requestValidator,
      IPdfGeneratorClient pdfGeneratorClient,
      IPolarisBlobStorageService blobStorageService,
      ILogger<GenerateThumbnail> logger)
    {
      _jsonConvertWrapper = jsonConvertWrapper;
      _requestValidator = requestValidator;
      _pdfGeneratorClient = pdfGeneratorClient;
      _blobStorageService = blobStorageService;
      _logger = logger;
    }

    [FunctionName(nameof(GenerateThumbnail))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.GenerateThumbnail)]
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

        var response = await GetTrackerDocument(client, caseId, new PolarisDocumentId(documentId), _logger, currentCorrelationId, nameof(GenerateThumbnail));
        var document = response.CmsDocument;

        var content = await req.Content.ReadAsStringAsync();
        var thumbnailDocumentRequest = _jsonConvertWrapper.DeserializeObject<GenerateThumbnailRequestDto>(content);

        using var documentStream = await _blobStorageService.GetDocumentAsync(document.PdfBlobName, currentCorrelationId);

        using var memoryStream = new MemoryStream();
        await documentStream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();
        var base64Document = Convert.ToBase64String(bytes);

        var thumbnailRequest = new GenerateThumbnailWithDocumentDto
        {
          Document = base64Document,
          VersionId = document.CmsVersionId,
          FileName = document.CmsOriginalFileName,
          ThumbnailParams = thumbnailDocumentRequest.ThumbnailParams
        };

        var validationResult = await _requestValidator.ValidateAsync(thumbnailRequest);
        if (!validationResult.IsValid)
          throw new BadRequestException(validationResult.FlattenErrors(), nameof(thumbnailRequest));

        var thumbnail = await _pdfGeneratorClient.GenerateThumbnail(caseUrn, caseId, documentId, thumbnailRequest, currentCorrelationId);

        return new FileStreamResult(thumbnail, ContentType.Jpeg);
      }
      catch (Exception ex)
      {
        return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(GenerateThumbnail), currentCorrelationId, ex);
      }
    }
  }
}