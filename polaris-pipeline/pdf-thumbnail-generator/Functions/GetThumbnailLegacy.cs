
using Common.Configuration;
using Common.Handlers;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Common.Constants;
using Common.Services.BlobStorage;
using Microsoft.Extensions.Configuration;
using Azure;

namespace pdf_thumbnail_generator.Functions
{ 
    public class GetThumbnailLegacy
    { 
        private readonly ILogger<GetThumbnailLegacy> _logger; 
        private readonly IExceptionHandler _exceptionHandler; 
        private readonly IPolarisBlobStorageService _blobStorageServiceContainerThumbnails;

        public GetThumbnailLegacy(ILogger<GetThumbnailLegacy> logger, IExceptionHandler exceptionHandler, Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, IConfiguration configuration) 
        { 
            _logger = logger; 
            _exceptionHandler = exceptionHandler; 
            _blobStorageServiceContainerThumbnails = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameThumbnails] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
        }

        [Function(nameof(GetThumbnailLegacy))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.ThumbnailLegacy)] HttpRequest req, 
            string caseUrn, int caseId, string materialId, int documentId, int maxDimensionPixel, int pageIndex)
        { 
            Guid currentCorrelationId = default;

            try
            { 
                currentCorrelationId = req.Headers.GetCorrelationId();

                var thumbnailBlobId = new BlobIdType(caseId, materialId, documentId, BlobType.Thumbnail);
                
                var imageStream = await _blobStorageServiceContainerThumbnails.GetBlobAsync(thumbnailBlobId);

                if (imageStream == null)
                    return new NotFoundResult();
                
                return new FileStreamResult(imageStream, ContentType.Jpeg);
            }
            catch (RequestFailedException)
            {
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                return _exceptionHandler.HandleExceptionNew(ex, currentCorrelationId, nameof(GetThumbnailLegacy), _logger);
            }
        }
    }
}