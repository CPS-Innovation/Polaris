using Aspose.Pdf;
using Common.Services.BlobStorageService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using pdf_thumbnail_generator.Durable.Payloads;
using pdf_thumbnail_generator.Services.ThumbnailGeneration;
using Common.Streaming;

namespace pdf_thumbnail_generator.Durable.Activity
{
  public class InitiateGenerateThumbnail
  {
    private readonly ILogger<InitiateGenerateThumbnail> _logger;
    private readonly IPolarisBlobStorageService _blobStorageServiceContainerDocuments;
    private readonly IPolarisBlobStorageService _blobStorageServiceContainerThumbnails;
    private readonly IThumbnailGenerationService _thumbnailGenerationService;

    public InitiateGenerateThumbnail(
        ILogger<InitiateGenerateThumbnail> logger,
        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
        IThumbnailGenerationService thumbnailGenerationService)
    {
      _logger = logger;
      _blobStorageServiceContainerDocuments = blobStorageServiceFactory("Documents");
      _blobStorageServiceContainerThumbnails = blobStorageServiceFactory("Thumbnails");
      _thumbnailGenerationService = thumbnailGenerationService;
    }


    [Function(nameof(InitiateGenerateThumbnail))]
    public async Task Run([ActivityTrigger] ThumbnailOrchestrationPayload payload)
    {
      var pdfBlobName = BlobNameHelper.GetBlobName(payload.CmsCaseId, payload.DocumentId, BlobNameHelper.BlobType.Pdf);
      var blobStream = await _blobStorageServiceContainerDocuments.GetDocumentVersionAsync(pdfBlobName, payload.VersionId.ToString());
      var stream = await blobStream.EnsureSeekableAsync();
      var document = new Document(stream);

      if (payload.PageIndex.HasValue)
      {
        await GenerateAndUploadThumbnailAsync(document, payload, payload.PageIndex.Value);
      }
      else
      {
        var tasks = new List<Task>();
        for (int pageIndex = 0; pageIndex < document.Pages.Count; pageIndex++)
        {
          tasks.Add(GenerateAndUploadThumbnailAsync(document, payload, pageIndex));
        }
        await Task.WhenAll(tasks);
      }
    }

    private async Task GenerateAndUploadThumbnailAsync(Document document, ThumbnailOrchestrationPayload payload, int pageIndex)
    {
      // aspose is 1 based page index
      var asposePageIndex = pageIndex + 1;
      using var stream = _thumbnailGenerationService.GenerateThumbnail(document.Pages[asposePageIndex], payload.MaxDimensionPixel, payload.CorrelationId);
      stream.Position = 0;

      var thumbnailBlobName = BlobNameHelper.GetBlobName(
          payload.CmsCaseId,
          payload.DocumentId,
          BlobNameHelper.BlobType.Thumbnails,
          payload.VersionId,
          pageIndex,
          payload.MaxDimensionPixel);

      await _blobStorageServiceContainerThumbnails.UploadDocumentAsync(stream, thumbnailBlobName);
    }
  }
}
