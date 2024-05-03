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
        private readonly IConfiguration _configuration;
        private readonly IPiiEntityMapper _piiEntityMapper;
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public PiiService(IConfiguration configuration, IPiiEntityMapper piiEntityMapper, IPolarisBlobStorageService blobStorageService, IJsonConvertWrapper jsonConvertWrapper)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _piiEntityMapper = piiEntityMapper ?? throw new ArgumentNullException(nameof(piiEntityMapper));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            // To come for config value...
            var piiCategoriesConfigValue = ""; //"Person;PersonType;PhoneNumber;Organization;Address;Email;";
            _piiCategories = piiCategoriesConfigValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
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
            if (piiChunks == null || piiResults == null) return null;

            var results = new List<ReconciledPiiEntity>();

            var piiToProcess = piiResults.PiiResultCollection.SelectMany(result => result.Items).ToList();

            foreach (var (item, itemIndex) in piiToProcess.Select((item, itemIndex) => (item, itemIndex)))
            {
                foreach (var piiEntity in item.Entities)
                {
                    var words = piiEntity.GetWordsWithOffset();
                    var chunk = piiChunks[itemIndex];

                    foreach (var (text, offset) in words)
                    {
                        var chunkLine = chunk.Lines.Where(x => x.ContainsOffset(offset)).SingleOrDefault();
                        var ocrWord = chunkLine.GetWord(text, offset);

                        if (ocrWord != null)
                            results.Add(new ReconciledPiiEntity(chunkLine, ocrWord, piiEntity.Category, chunk.DocumentId));
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
                    PiiCategory = entity.PiiCategory
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

            return _jsonConvertWrapper.DeserializeObject<PiiEntitiesWrapper>(piiStreamReader.ReadToEnd());
        }
    }
}