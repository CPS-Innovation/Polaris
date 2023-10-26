using System;
using System.Threading.Tasks;
using Common.Domain.Extensions;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using Common.Wrappers.Contracts;
using Microsoft.Extensions.Logging;

namespace pdf_generator.Services.DocumentRedaction
{
    public class DocumentRedactionService : IDocumentRedactionService
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly IUploadFileNameFactory _uploadFileNameFactory;
        private readonly IRedactionProvider _redactionProvider;
        private readonly ILogger<DocumentRedactionService> _logger;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public DocumentRedactionService(
            IPolarisBlobStorageService blobStorageService,
            IUploadFileNameFactory uploadFileNameFactory,
            IRedactionProvider redactionProvider,
            IJsonConvertWrapper jsonConvertWrapper,
            ILogger<DocumentRedactionService> logger)
        {
            _polarisBlobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _uploadFileNameFactory = uploadFileNameFactory;
            _redactionProvider = redactionProvider;
            _jsonConvertWrapper = jsonConvertWrapper;
            _logger = logger;
        }

        public async Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequestDto redactPdfRequest, Guid correlationId)
        {
            try
            {
                _logger.LogMethodEntry(correlationId, nameof(RedactPdfAsync), redactPdfRequest.ToJson());

                var documentStream = await _polarisBlobStorageService.GetDocumentAsync(redactPdfRequest.FileName, correlationId);

                var redactedDocumentStream = _redactionProvider.Redact(documentStream, redactPdfRequest, correlationId);

                var uploadFileName = _uploadFileNameFactory.BuildUploadFileName(redactPdfRequest.FileName);

                await _polarisBlobStorageService.UploadDocumentAsync(
                    redactedDocumentStream,
                    uploadFileName,
                    redactPdfRequest.CaseId.ToString(),
                    redactPdfRequest.PolarisDocumentId,
                    redactPdfRequest.VersionId.ToString(),
                    correlationId);

                var redactPdfRequestJsonStream = _jsonConvertWrapper.SerializeObjectToStream(redactPdfRequest);
                var redactPdfRequestFileName = _uploadFileNameFactory.BuildRedactionJsonFileName(uploadFileName);

                await _polarisBlobStorageService.UploadRedactionJsonAsync(
                    redactPdfRequestJsonStream,
                    redactPdfRequestFileName,
                    correlationId
                );

                return new RedactPdfResponse
                {
                    Succeeded = true,
                    RedactedDocumentName = uploadFileName
                };
            }
            catch (Exception ex)
            {
                return new RedactPdfResponse
                {
                    Succeeded = false,
                    Message = ex.Message
                };
            }
        }
    }
}