using Aspose.Pdf;
using Aspose.Pdf.Devices;
using Microsoft.Extensions.Logging;

namespace pdf_thumbnail_generator.Services.ThumbnailGeneration
{
  public class ThumbnailGenerationService : IThumbnailGenerationService
  {
    private readonly ILogger<ThumbnailGenerationService> _logger;

    public ThumbnailGenerationService(ILogger<ThumbnailGenerationService> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Stream GenerateThumbnail(Page page, int maxDimension, Guid correlationId)
    {
      try
      {
        var memoryStream = new MemoryStream();
        var pageRect = page.GetPageRect(true);

        // Determine if the page is in landscape or portrait orientation
        var isLandscape = pageRect.Width > pageRect.Height;

        int thumbnailWidth;
        int thumbnailHeight;

        if (isLandscape)
        {
          thumbnailWidth = maxDimension;
          thumbnailHeight = (int)(pageRect.Height * maxDimension / pageRect.Width);
        }
        else
        {
          thumbnailHeight = maxDimension;
          thumbnailWidth = (int)(pageRect.Width * maxDimension / pageRect.Height);
        }

        JpegDevice jpegDevice = new JpegDevice(thumbnailWidth, thumbnailHeight);
        jpegDevice.Process(page, memoryStream);

        return memoryStream;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error generating thumbnail for correlation ID {CorrelationId}", correlationId);
        throw;
      }
    }
  }
}
