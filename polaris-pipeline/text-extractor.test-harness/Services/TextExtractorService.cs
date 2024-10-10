using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using coordinator.Clients.TextExtractor;
using Common.Dto.Document;
using TextExtractor.TestHarness.Constants;
using TextExtractor.TestHarness.Extensions;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;

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
                $"CMS-{documentRef}",
                documentRef, fileExtension, filename);

            var payload = new CaseDocumentOrchestrationPayload(null, Guid.NewGuid(), TestProperties.CaseUrn, TestProperties.CmsCaseId, cmsDocumentEntity, null, null, DocumentDeltaType.RequiresIndexing);

            try
            {
                using (var documentStream = File.Open(filePath, FileMode.Open))
                    await _textExtractorClient.ExtractTextAsync(payload.PolarisDocumentId,
                        payload.CmsCaseUrn,
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
            string polarisDocumentId,
            string cmsDocumentId,
            string fileExtension,
            string cmsOriginalFileName)
        {
            var cmsDocumentEntity = new CmsDocumentEntity(
                polarisDocumentId: polarisDocumentId,
                cmsDocumentId: cmsDocumentId,
                cmsVersionId: 1,
                cmsDocType: new DocumentTypeDto(),
                path: null,
                cmsFileCreatedDate: DateTime.Now.ToString(),
                cmsOriginalFileName: cmsOriginalFileName,
                presentationTitle: TestProperties.PresentationTitle,
                isOcrProcessed: false,
                isDispatched: false,
                categoryListOrder: null,
                polarisParentDocumentId: null,
                cmsParentDocumentId: null,
                witnessId: null,
                presentationFlags: new Common.Dto.FeatureFlags.PresentationFlagsDto(),
                hasFailedAttachments: false);

            return JsonSerializer.Serialize(cmsDocumentEntity);
        }
    }
}