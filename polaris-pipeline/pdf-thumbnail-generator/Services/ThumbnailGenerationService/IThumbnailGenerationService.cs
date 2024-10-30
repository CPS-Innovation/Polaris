using Aspose.Pdf;

namespace pdf_thumbnail_generator.Services.ThumbnailGenerationService
{ 
    public interface IThumbnailGenerationService 
    { 
        public Stream GenerateThumbnail(Page page, int maxDimension, Guid correlationId);
    }
}