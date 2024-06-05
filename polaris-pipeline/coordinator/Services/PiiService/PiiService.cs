using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using Common.Domain.Pii;
using Common.Services.BlobStorageService;
using Common.Wrappers;
using coordinator.Domain;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Helpers;
using coordinator.Services.OcrResultsService;
using Microsoft.Extensions.Configuration;

namespace coordinator.Services.PiiService
{
    public class PiiService : IPiiService
    {
        private const int DocumentSize = 5;
        private readonly string[] _piiCategories;
        private readonly IPiiEntityMapper _piiEntityMapper;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IPiiAllowedListService _piiAllowedList;

        public PiiService(IPiiEntityMapper piiEntityMapper, IPolarisBlobStorageService blobStorageService, IJsonConvertWrapper jsonConvertWrapper, IConfiguration configuration, IPiiAllowedListService allowedList)
        {
            _piiEntityMapper = piiEntityMapper ?? throw new ArgumentNullException(nameof(piiEntityMapper));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _piiAllowedList = allowedList ?? throw new ArgumentNullException(nameof(allowedList));
            _piiCategories = configuration["PiiCategories"].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public IEnumerable<PiiRequestDto> CreatePiiRequests(List<PiiChunk> piiChunks)
        {
            var piiRequests = new List<PiiRequestDto>();
            var processedCount = 0;
            var chunksToProcess = piiChunks.Count;

            while (processedCount < chunksToProcess)
            {
                var documents = piiChunks
                    .Select(x => x)
                    .Skip(processedCount)
                    .Take(DocumentSize).ToList();

                piiRequests.Add(new PiiRequestDto(_piiCategories, documents));
                processedCount += documents.Count;
            }

            return piiRequests;
        }

        public IEnumerable<PiiLine> ReconcilePiiResults(IList<PiiChunk> piiChunks, PiiEntitiesWrapper piiResults)
        {
            if (piiChunks == null || piiResults == null) return new List<PiiLine>();

            var results = new List<ReconciledPiiEntity>();

            var piiToProcess = piiResults.PiiResultCollection.SelectMany(result => result.Items).ToList();

            foreach (var (item, itemIndex) in piiToProcess.Select((item, itemIndex) => (item, itemIndex)))
            {
                foreach (var piiEntity in item.Entities)
                {
                    var entityGroupId = Guid.NewGuid();
                    var words = piiEntity.GetWordsWithOffset();
                    var chunk = piiChunks[itemIndex];

                    foreach (var (text, offset) in words)
                    {
                        if (_piiAllowedList.Contains(text, piiEntity.Category)) continue;

                        var chunkLine = chunk.Lines.Single(x => x.ContainsOffset(offset));
                        var ocrWord = chunkLine.GetWord(text, offset);
                        var redactionType = GetRedactionTypeCategoryMapping(piiEntity.Category);

                        if (ocrWord != null)
                            results.Add(new ReconciledPiiEntity(chunkLine, ocrWord, piiEntity.Category, redactionType, chunk.DocumentId, entityGroupId));
                    }
                }
            }

            return MapReconcilledPiiToResponse(results);
        }

        public PiiEntitiesWrapper MapPiiResults(RecognizePiiEntitiesResultCollection[] piiResults)
        {
            return new PiiEntitiesWrapper
            {
                PiiResultCollection = piiResults.Select(result => _piiEntityMapper.MapCollection(result))
            };
        }

        internal static IEnumerable<PiiLine> MapReconcilledPiiToResponse(List<ReconciledPiiEntity> piiEntities)
        {
            var results = new List<PiiLine>();

            foreach (var entity in piiEntities)
            {
                var piiLine = results.SingleOrDefault(x => x.AccumulativeLineIndex == entity.AccumulativeLineIndex);

                if (piiLine == null)
                {
                    piiLine = new PiiLine
                    {
                        PolarisDocumentId = entity.PolarisDocumentId,
                        PageIndex = entity.PageIndex,
                        LineIndex = entity.LineIndex,
                        AccumulativeLineIndex = entity.AccumulativeLineIndex,
                        PageHeight = entity.PageHeight,
                        PageWidth = entity.PageWidth,
                        Text = entity.LineText,
                        Id = Guid.NewGuid().ToString(),
                        Words = new List<PiiWord>()
                    };

                    var lineWords = entity.LineText.Split(' ');
                    foreach (var lineWord in lineWords)
                    {
                        piiLine.Words.Add(new PiiWord { Text = lineWord });
                    }

                    results.Add(piiLine);
                }

                var word = piiLine.Words
                    .Where(x => x.Text == entity.Word.Text)
                    .Where(x => x.BoundingBox == null)
                    .FirstOrDefault();

                var wordIndex = piiLine.Words.IndexOf(word);
                word = new PiiWord
                {
                    Text = entity.Word.Text,
                    BoundingBox = entity.Word.BoundingBox,
                    PiiCategory = entity.PiiCategory,
                    PiiGroupId = entity.EntityGroupId,
                    RedactionType = entity.RedactionType
                };
                if (wordIndex != -1)
                    piiLine.Words[wordIndex] = word;
            }

            return results;
        }

        public async Task<PiiEntitiesWrapper> GetPiiResultsFromBlob(int caseId, string polarisDocumentId, Guid correlationId)
        {
            Stream piiStream;

            try
            {
                var piiBlobName = BlobNameHelper.GetBlobName(caseId, polarisDocumentId, BlobNameHelper.BlobType.Pii);
                piiStream = await _blobStorageService.GetDocumentAsync(piiBlobName, correlationId);
            }
            catch (Exception)
            {
                return null; // return null for now;
            }

            var piiStreamReader = new StreamReader(piiStream);

            return _jsonConvertWrapper.DeserializeObject<PiiEntitiesWrapper>(await piiStreamReader.ReadToEndAsync());
        }

        private static string GetRedactionTypeCategoryMapping(string piiCategory)
        {
            PiiToRedactionLogCategoryMappings.TryGetValue(piiCategory, out var category);

            return category ?? "Other";
        }

        private static Dictionary<string, string> PiiToRedactionLogCategoryMappings =>
            new()
            {
                { PiiCategory.Address,                    "Address" },
                { PiiCategory.Email,                      "Email Address"},
                { PiiCategory.IPAddress,                  "Location" },
                { PiiCategory.Person,                     "Named Individual" },
                { PiiCategory.UKNationalHealthNumber,     "NHS number" },
                { PiiCategory.UKNationalInsuranceNumber,  "NI number" },
                { PiiCategory.PersonType,                 "Occupation" },
                { PiiCategory.PhoneNumber,                "Phone number" },
                { PiiCategory.CreditCardNumber,           "Other" },
                { PiiCategory.EUDriversLicenseNumber,     "Other" },
                { PiiCategory.UKDriversLicenseNumber,     "Other" },
                { PiiCategory.EUPassportNumber,           "Other" },
                { PiiCategory.USUKPassportNumber,         "Other" }
            };
    }
}