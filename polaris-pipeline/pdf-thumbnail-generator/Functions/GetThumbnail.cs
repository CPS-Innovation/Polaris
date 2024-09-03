

using Common.Configuration;
using Common.Handlers;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Common.Services.BlobStorageService;
using Common.Constants;
using Microsoft.WindowsAzure.Storage;

namespace pdf_thumbnail_generator.Functions
{
  public class GetThumbnail
  {
    private readonly ILogger<GetThumbnail> _logger;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly IPolarisBlobStorageService _blobStorageService;

    public GetThumbnail(
      ILogger<GetThumbnail> logger,
      IExceptionHandler exceptionHandler,
      IPolarisBlobStorageService blobStorageService)
    {
      _logger = logger;
      _exceptionHandler = exceptionHandler;
      _blobStorageService = blobStorageService;
    }

    [Function(nameof(GetThumbnail))]
    public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Thumbnail)] HttpRequest req,
      string caseUrn,
      int caseId,
      string documentId,
      int versionId,
      int maxDimensionPixel,
      int pageIndex)
    {
      Guid currentCorrelationId = default;
      try
      {
        currentCorrelationId = req.Headers.GetCorrelationId();

        var blobName = BlobNameHelper.GetBlobName(caseId, documentId, BlobNameHelper.BlobType.Thumbnails, versionId, pageIndex, maxDimensionPixel);

        var imageStream = await _blobStorageService.GetDocumentAsync(blobName, currentCorrelationId);

        if (imageStream == null)
        {
          return new NotFoundResult();
        }

        return new FileStreamResult(imageStream, ContentType.Jpeg);
      }
      catch (StorageException)
      {
        return new NotFoundResult();
      }
      catch (Exception ex)
      {
        return _exceptionHandler.HandleExceptionNew(ex, currentCorrelationId, nameof(GetThumbnail), _logger);
      }
    }
  }
}