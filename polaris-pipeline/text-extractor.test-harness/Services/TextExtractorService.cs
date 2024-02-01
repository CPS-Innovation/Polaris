using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Clients.Contracts;
using Common.Domain.Entity;
using Common.Dto.Document;
using Common.ValueObjects;
using coordinator.Domain;
using TextExtractor.TestHarness.Constants;
using TextExtractor.TestHarness.Extensions;

namespace TextExtractor.TestHarness.Services
{
    public class TextExtractorService : ITextExtractorService
    {
        private readonly ITextExtractorClient _textExtractorClient;

        public TextExtractorService(ITextExtractorClient textExtractorClient)
        {
            _textExtractorClient = textExtractorClient;
        }

        public async Task ExtractAndStoreDocument(string filename)
        {
            var filePath = filename.GetFilePath();
            var documentRef = $"TEST-{filename.Split(".")[0].ToUpperInvariant()}";
            var fileExtension = Path.GetExtension(filePath);

            var cmsDocumentEntity = SerializedCmsDocumentDto(
                new PolarisDocumentId(PolarisDocumentType.CmsDocument, documentRef),
                    documentRef, fileExtension, filename);

            var payload = new CaseDocumentOrchestrationPayload(null, Guid.NewGuid(), TestProperties.CaseUrn, TestProperties.CmsCaseId, cmsDocumentEntity, null, null);

            try
            {
                using (var documentStream = File.Open(filePath, FileMode.Open))
                    await _textExtractorClient.ExtractTextAsync(payload.PolarisDocumentId,
                        payload.CmsCaseId,
                        payload.CmsDocumentId,
                        payload.CmsVersionId,
                        payload.BlobName,
                        payload.CorrelationId,
                        documentStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string SerializedCmsDocumentDto(
            PolarisDocumentId polarisDocumentId,
            string cmsDocumentId,
            string fileExtension,
            string cmsOriginalFileName)
        {
            var cmsDocumentEntity = new CmsDocumentEntity(
                polarisDocumentId: polarisDocumentId,
                polarisDocumentVersionId: 1,
                cmsDocumentId: cmsDocumentId,
                cmsVersionId: 1,
                cmsDocType: new DocumentTypeDto(),
                path: null,
                fileExtension: fileExtension,
                cmsFileCreatedDate: DateTime.Now.ToString(),
                cmsOriginalFileName: cmsOriginalFileName,
                presentationTitle: TestProperties.PresentationTitle,
                isOcrProcessed: false,
                isDispatched: false,
                categoryListOrder: null,
                polarisParentDocumentId: null,
                cmsParentDocumentId: null,
                witnessId: null,
                presentationFlags: new Common.Dto.FeatureFlags.PresentationFlagsDto());

            return JsonSerializer.Serialize(cmsDocumentEntity);
        }
    }
}