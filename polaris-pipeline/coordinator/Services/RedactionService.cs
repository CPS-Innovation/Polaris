using Common.Configuration;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Exceptions;
using Common.Extensions;
using Common.Services.BlobStorage;
using coordinator.Clients.PdfRedactor;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Services.CaseUrnResolver;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace coordinator.Services
{
   public class RedactionService : IRedactionService
    {
        private readonly IValidator<RedactPdfRequestWithDocumentDto> requestValidator;
        private readonly IPdfRedactorClient redactionClient;
        private readonly IPolarisBlobStorageService blobStorageService;
        private readonly IMdsArgFactory mdsArgFactory;
        private readonly IMdsClient mdsClient;
        private readonly ICaseUrnResolver caseUrnResolver;

        public RedactionService(
            IValidator<RedactPdfRequestWithDocumentDto> requestValidator,
            IPdfRedactorClient redactionClient,
            Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
            IMdsArgFactory mdsArgFactory,
            IConfiguration configuration,
            IMdsClient mdsClient,
            ICaseUrnResolver caseUrnResolver)
        {
            this.requestValidator = requestValidator.ExceptionIfNull();
            this.redactionClient = redactionClient.ExceptionIfNull();
            this.blobStorageService =
                blobStorageServiceFactory(
                    configuration[StorageKeys.BlobServiceContainerNameDocuments]
                    ?? string.Empty).ExceptionIfNull();

            this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
            this.mdsClient = mdsClient.ExceptionIfNull();
            this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
        }

        public async Task<Stream> ProcessAsync(
            int caseId,
            string materialId,
            long documentId,
            RedactPdfRequestDto request,
            CmsAuthValues cmsAuthValues,
            Guid correlationId,
            CancellationToken cancellationToken)
        {
            var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(
                caseId,
                cmsAuthValues,
                cancellationToken);

            using var documentStream =
                await this.blobStorageService.GetBlobAsync(
                    new BlobIdType(
                        caseId,
                        materialId,
                        documentId,
                        BlobType.Pdf));

            using var memoryStream = new MemoryStream();

            await documentStream.CopyToAsync(
                memoryStream,
                cancellationToken);

            var bytes = memoryStream.ToArray();

            Stream document = null;

            if (request.RedactionDefinitions.Count != 0)
            {
                document = await RedactAsync(
                    bytes,
                    request,
                    caseId,
                    materialId,
                    documentId,
                    correlationId,
                    cancellationToken);
            }

            if (request.DocumentModifications.Count != 0)
            {
                document = await ModifyAsync(
                    document ?? new MemoryStream(bytes),
                    request,
                    caseId,
                    materialId,
                    documentId,
                    correlationId,
                    cancellationToken);
            }

            var arg = this.mdsArgFactory.CreateDocumentVersionArgDto(
                cmsAuthValues.CmsAuthFullValue,
                correlationId: correlationId,
                caseUrn,
                caseId: caseId,
                DocumentNature.ToNumericDocumentId(
                    materialId,
                    DocumentNature.Types.Document),
                documentId);

            var result = await this.mdsClient.UploadPdfAsync(
                arg,
                document,
                cancellationToken);

            if (result.StatusCode == HttpStatusCode.Gone ||
                result.StatusCode == HttpStatusCode.RequestEntityTooLarge)
            {
                throw new HttpRequestException(
                    $"Document upload failed with status code {result.StatusCode}");
            }

            return document;
        }
        private async Task<Stream> RedactAsync(
                                        byte[] bytes,
                                        RedactPdfRequestDto request,
                                        int caseId,
                                        string materialId,
                                        long documentId,
                                        Guid correlationId,
                                        CancellationToken cancellationToken)
        {
            var redactionRequest = new RedactPdfRequestWithDocumentDto
            {
                Document = Convert.ToBase64String(bytes),
                RedactionDefinitions = request.RedactionDefinitions
            };

            var validationResult =
                await this.requestValidator.ValidateAsync(
                    redactionRequest,
                    cancellationToken);

            if (!validationResult.IsValid)
            {
                throw new BadRequestException(
                    validationResult.FlattenErrors(),
                    nameof(request));
            }

            var stream = await this.redactionClient.RedactPdfAsync(
                caseUrn: null,
                caseId,
                materialId,
                documentId,
                redactionRequest,
                correlationId,
                isLegacy: false);

            return stream
                ?? throw new InvalidOperationException(
                    $"Error saving redaction details for {caseId}, materialId {materialId}");
        }

        private async Task<Stream> ModifyAsync(
                                        Stream documentStream,
                                        RedactPdfRequestDto request,
                                        int caseId,
                                        string materialId,
                                        long documentId,
                                        Guid correlationId,
                                        CancellationToken cancellationToken)
        {
            using var memoryStream = new MemoryStream();

            if (documentStream.CanSeek)
            {
                documentStream.Position = 0;
            }

            await documentStream.CopyToAsync(
                memoryStream,
                cancellationToken);

            var modificationRequest = new ModifyDocumentWithDocumentDto
            {
                Document = Convert.ToBase64String(memoryStream.ToArray()),
                DocumentModifications = request.DocumentModifications,
                VersionId = request.VersionId
            };

            return await this.redactionClient.ModifyDocument(
                       caseUrn: null,
                       caseId,
                       materialId,
                       documentId,
                       modificationRequest,
                       correlationId,
                       isLegacy: false)
                   ?? throw new InvalidOperationException(
                       $"Error modifying document for {caseId}, materialId {materialId}");
        }
    }
}
