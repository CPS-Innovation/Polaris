using Aspose.Pdf;
using Aspose.Pdf.Devices;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace pdf_thumbnail_generator.Services.ThumbnailGeneration
{
  public class ThumbnailGenerationService : IThumbnailGenerationService
  {
    private readonly ILogger<ThumbnailGenerationService> _logger;
    private readonly int thumbnailHeight = 1000;
    private readonly int thumbnailWidth = 1000;

    public ThumbnailGenerationService(
        ILogger<ThumbnailGenerationService> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Stream GenerateThumbnail(Page page, Guid correlationId)
    {
      try
      {
        var memoryStream = new MemoryStream();
        JpegDevice jpegDevice = new JpegDevice(thumbnailHeight, thumbnailWidth);
        jpegDevice.Process(page, memoryStream);

        return memoryStream;
      }
      catch (Exception ex)
      {
        _logger.LogMethodError(correlationId, nameof(GenerateThumbnail), ex.Message, ex);
        throw;
      }
    }
  }
}