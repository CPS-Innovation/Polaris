using Aspose.Pdf;
using Common.Services.BlobStorage;
using Microsoft.Azure.Functions.Worker;
using pdf_thumbnail_generator.Durable.Payloads;
using Common.Streaming;
using pdf_thumbnail_generator.Services.ThumbnailGenerationService;

namespace pdf_thumbnail_generator.Durable.Activity
{
  public class InitiateGenerateThumbnail
  {
      private readonly IPolarisBlobStorageService _blobStorageServiceContainerDocuments;
      private readonly IPolarisBlobStorageService _blobStorageServiceContainerThumbnails;
      private readonly IThumbnailGenerationService _thumbnailGenerationService;

      public InitiateGenerateThumbnail(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, IThumbnailGenerationService thumbnailGenerationService)
      {
          _blobStorageServiceContainerDocuments = blobStorageServiceFactory("Documents");
          _blobStorageServiceContainerThumbnails = blobStorageServiceFactory("Thumbnails");
          _thumbnailGenerationService = thumbnailGenerationService;
      }


    [Function(nameof(InitiateGenerateThumbnail))]
    public async Task Run([ActivityTrigger] ThumbnailOrchestrationPayload payload)
    { 
        var targetBlobId = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Pdf); 
        var blobStream = await _blobStorageServiceContainerDocuments.GetBlobAsync(targetBlobId);
        var stream = await blobStream.EnsureSeekableAsync();
        var document = new Document(stream);

        if (payload.PageIndex.HasValue)
        {
            await GenerateAndUploadThumbnailAsync(document, payload, payload.PageIndex.Value);
        }
        else
        { 
            var tasks = new List<Task>();
            for (var pageIndex = 0; pageIndex < document.Pages.Count; pageIndex++)
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
        await using var stream = _thumbnailGenerationService.GenerateThumbnail(document.Pages[asposePageIndex], payload.MaxDimensionPixel, payload.CorrelationId);
        stream.Position = 0;

        var thumbnailBlobId = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Thumbnail);
        await _blobStorageServiceContainerThumbnails.UploadBlobAsync(stream, thumbnailBlobId, pageIndex, payload.MaxDimensionPixel);
    }
  }
}
