
using Common.Configuration;
using Common.Handlers;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Common.Constants;
using Common.Helpers;
using Common.Services.BlobStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;

namespace pdf_thumbnail_generator.Functions
{ 
    public class GetThumbnail
    { 
        private readonly ILogger<GetThumbnail> _logger; 
        private readonly IExceptionHandler _exceptionHandler; 
        private readonly IPolarisBlobStorageService _blobStorageServiceContainerThumbnails;

        public GetThumbnail(ILogger<GetThumbnail> logger, IExceptionHandler exceptionHandler, Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, IConfiguration configuration) 
        { 
            _logger = logger; 
            _exceptionHandler = exceptionHandler; 
            _blobStorageServiceContainerThumbnails = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameThumbnails] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
        }

        [Function(nameof(GetThumbnail))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Thumbnail)] HttpRequest req, 
            string caseUrn, int caseId, string documentId, int versionId, int maxDimensionPixel, int pageIndex)
        { 
            Guid currentCorrelationId = default;
            
            try
            { 
                currentCorrelationId = req.Headers.GetCorrelationId();

                var thumbnailBlobId = new BlobIdType(caseId, documentId, versionId, BlobType.Thumbnail);
                
                var imageStream = await _blobStorageServiceContainerThumbnails.GetBlobAsync(thumbnailBlobId);

                if (imageStream == null)
                    return new NotFoundResult();
                
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