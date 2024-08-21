using System;
using System.IO;
using System.Threading.Tasks;
using Common.Dto.Request;

namespace pdf_generator.Services.ThumbnailGeneration
{
  public interface IThumbnailGenerationService
  {
    public Task<Stream> GenerateThumbnail(string caseId, string documentId, GenerateThumbnailWithDocumentDto thumbnailDetails, Guid correlationId);
  }
}